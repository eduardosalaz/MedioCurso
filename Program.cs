using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using ScottPlot;

namespace MedioCurso
{

    internal class HilosvsTiempo
    {
        public readonly double Hilos;
        public readonly double Tiempo;
        public HilosvsTiempo(double hilos, double tiempo)
        {
            Hilos = hilos;
            Tiempo = tiempo;
        }
    }

    internal static class Program
    {
        private static double[][] _matrizResultado;

        static void Main()
        {

            Console.WriteLine("Ingrese el número de filas y columnas de la matriz: ");
            int nFilscols = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Ingrese el límite inferior del intervalo de aleatorios: ");
            int min = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Ingrese el límite superior del intervalo de aleatorios: ");
            int max = Convert.ToInt32(Console.ReadLine());

            double[][] matriz1 = new double[nFilscols][];

            for (var i = 0; i < matriz1.Length; i++)
            {
                // arreglo de arreglos
                matriz1[i] = new double[nFilscols];
            }


            double[][] matriz2 = new double[nFilscols][];

            for (var i = 0; i < matriz2.Length; i++)
            {
                // arreglo de arreglos
                matriz2[i] = new double[nFilscols];
            }


            var random = new Random();

            for (int i = 0; i < nFilscols; i++)
            {
                for (int j = 0; j < nFilscols; j++)
                {
                    matriz1[i][j] = random.Next(min, max);
                    matriz2[i][j] = random.Next(min, max);

                }
            }

            Console.WriteLine("Matriz 1 : ");
            for (int i = 0; i < nFilscols; i++)
            {
                for (int j = 0; j < nFilscols; j++)
                {
                    Console.Write(matriz1[i][j] + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("Matriz 2 : ");
            for (int i = 0; i < nFilscols; i++)
            {
                for (int j = 0; j < nFilscols; j++)
                {
                    Console.Write(matriz2[i][j] + " ");
                }
                Console.WriteLine();
            }



            var filasResultado = matriz1.Length; // filas de la primera
            Console.WriteLine(filasResultado);
            var columnasResultado = matriz2[0].Length; // cols de la segunda
            Console.WriteLine(columnasResultado);

            _matrizResultado = new double[filasResultado][];

            for (var i = 0; i < filasResultado; i++)
            {
                // arreglo de arreglos
                _matrizResultado[i] = new double[columnasResultado];
            }

            var contador = 0;

            var matriz2Flattened =
                new double[matriz2[0].Length * matriz2.Length]; //la flateneamos, o pasamos a 1 dimension
            //en este caso toma la length del primer arreglo que la compone (10) y la multiplica por los 3 arreglos que contiene
            //dando como resultado 30

            for (var j = 0; j < matriz2[0].Length; j++) //de 0 a 9
            {
                foreach (var elementos in matriz2) //agarra los 3 arreglos que hay adentro de matriz2
                {
                    matriz2Flattened[contador] =
                        elementos[j]; //pasa los elementos de la matriz2 a la matriz2flattened de 1 sola dimension
                    contador++;
                }
            }

            var nHilosInicial = 1;
            var n = 4; // n = iteraciones
            var numHilos = new int[n];

            for (var i = 0; i < n; i++)
            {
                numHilos[i] = nHilosInicial;
                nHilosInicial += 1;
            }

            var valoresGrafica = new List<HilosvsTiempo>();

            for (var iter = 0; iter < n; iter++)
            {
                var tiempo = new TimeSpan();
                var hilos = new Thread[numHilos[iter]]; //arreglo de hilos
                //hilos = [varHilos[0]] es decir, hilos = [1]
                //hilos = [varHilos[1]] = [2]
                //hilos = [3]
                //hilos = [4]
                //serializamos los hilos
                for (var i = 0; i < numHilos[iter]; i++)
                {
                    hilos[i] = new Thread(Multiplicar);
                }

                var bloque =
                    (int)Math.Ceiling((double)filasResultado /
                                       numHilos[iter]); //divide entre el numero de hilos actual
                var contadorBloque = 0;
                var contadorSecundario = 0;

                for (var k = 0; k < numHilos[iter]; k++)
                {
                    if (k == numHilos[iter] - 1)
                    {
                        //asignar faltante al ultimo hilo
                        bloque = filasResultado - contadorSecundario;
                    }

                    var bloqueTemporal = new double[bloque][]; // segmentar
                    for (var i = 0; i < bloque; i++)
                    {
                        //filas a los segmentos
                        bloqueTemporal[i] = matriz1[contadorBloque + i];
                    }

                    //filas = bloques, arrancan hilos y operar por bloques
                    for (var i = 0; i < bloque; i++)
                    {
                        var arregloTemporal = new double[bloqueTemporal[0].Length];
                        for (var j = 0; j < bloqueTemporal[0].Length; j++)
                        {
                            arregloTemporal[j] = bloqueTemporal[i][j];
                        }

                        var sw = Stopwatch.StartNew(); //lo mismo que currentMillis
                        hilos[k] = new Thread(Multiplicar);
                        hilos[k].Start(new object[]
                        {
                            arregloTemporal, matriz2Flattened, i + contadorBloque
                        }); //le pasamos al hilo el arreglo de objetos
                        hilos[k].Join(); //no funciona lo de isAlive muy bien
                        tiempo = tiempo + sw.Elapsed; //como el currentMillis()
                        //tiempo no es lo mismo que stopwatch
                    }

                    contadorBloque += bloque;
                    contadorSecundario += bloque;
                }

                valoresGrafica.Add(new HilosvsTiempo(numHilos[iter], tiempo.TotalMilliseconds));

                Console.WriteLine("Matriz resultante con: " + numHilos[iter] + ": ");
                for (int i = 0; i < filasResultado; i++)
                {
                    for (int j = 0; j < columnasResultado; j++)
                    {
                        Console.Write(_matrizResultado[i][j] + " ");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();

                _matrizResultado = new double[filasResultado][];
                for (var i = 0; i < filasResultado; i++)
                {
                    //arreglos de arreglos
                    _matrizResultado[i] = new double[columnasResultado];
                }
            }

            Graficar(valoresGrafica, filasResultado, columnasResultado);

            foreach (var i in valoresGrafica)
            {
                Console.WriteLine($"Hilos: {i.Hilos} Tiempo: {i.Tiempo}");
            }
        }

        //este metodo es el meollo del asunto
        private static void Multiplicar(object entradaObj)
        {
            //Console.WriteLine("entre al metodo");
            var entradaArreglo = (object[])entradaObj;
            var arr1 = (double[])entradaArreglo[0];
            var arr2 = (double[])entradaArreglo[1];
            var fila = (int)entradaArreglo[2];
            //Console.WriteLine(arr1);
            //Console.WriteLine(arr2);
            //Console.WriteLine(fila);

            var intervalo = arr1.Length;
            var contador = 0;
            double suma = 0;
            for (var i = 0; i < (arr2.Length / intervalo); i++)
            {
                for (var j = 0; j < intervalo; j++)
                {
                    suma += arr1[j] * arr2[contador];
                    contador++;
                }

                _matrizResultado[fila][i] = suma; // lo pasamos a la variable de clase
                suma = 0;
            }
        }

        private static void Graficar(List<HilosvsTiempo> valores, int filas, int cols)
        {
            var plt = new Plot(600, 400);
            var tiempos = new double[valores.Count];
            var hilos = new double[valores.Count];
            var j = 0;
            foreach (var i in valores)
            {
                tiempos[j] = i.Tiempo;
                hilos[j] = i.Hilos;
                j++;
            }
            //el eje de las x son los hilos usados, y el tiempo.

            plt.PlotScatter(hilos, tiempos);
            plt.Legend();

            plt.Title("Hilos/Tiempo");
            plt.XLabel("# Hilos");
            plt.YLabel("Tiempo (s)");
            var path = $"matriz_{filas}x{cols}.png";
            try
            {
                plt.SaveFig(path);
                Console.WriteLine("Gráfica generada y guardada");
            }
            catch (Exception)
            {
                Console.WriteLine("Error");
            }
        }
    }
}
