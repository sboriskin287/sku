using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;

namespace testdll
{
class Program
    {
        static void Main(string[] args)
        {
            Simulation sim = new Simulation();
            using (NamedPipeClientStream pipe = new NamedPipeClientStream(".", "{E8B5BDF5-725C-4BF4-BCA4-2427875DF2E0}", PipeDirection.InOut))
            {
                pipe.Connect();
                if (pipe.IsConnected)
                {
                    using (StreamReader sr = new StreamReader(pipe))
                    {
                        String tmp;
                        byte bt;
                        while (true)
                        {

                            if ((tmp = sr.ReadLine()) != null)
                            {
                                if (tmp.Contains("step"))
                                {
                                    Console.WriteLine(tmp);
                                    sim.Step();
                                }
                                if (tmp.Contains("set"))
                                {
                                    Console.WriteLine(tmp);
                                    for (int i = 4; i < tmp.Length; i++ )
                                    {
                                        if (tmp[i] == ' ')
                                        {
                                            sim.Set(int.Parse(tmp.Substring(4, i-4)), tmp[i+1] == '0' ? false : true);
                                            break;
                                        }
                                    }
                                }
                                if (tmp.Contains("get"))
                                {
                                    for (int i = 4; i < tmp.Length; i++)
                                    {
                                        if (tmp[i] == ' ')
                                        {
                                            bt = (sim.Get(int.Parse(tmp.Substring(4, i-4))) == false ? (byte)0 : (byte)1);
                                            pipe.WriteByte(bt);
                                            Console.WriteLine(tmp + "-> " + bt.ToString());
                                            break;
                                        }
                                        if (i == tmp.Length-1)
                                        {
                                            Console.WriteLine(tmp + "-> 0");
                                            pipe.WriteByte((byte)0);
                                        }
                                    }
                                    pipe.Flush();
                                }
                                if (tmp.Contains("exit") || !pipe.IsConnected)
                                {
                                    break;
                                }
                            }
                        }
                        sr.Close();
                    }
                }
                pipe.Close();
            }
        }
    }
	enum simulDefines
    {
	$
	};
	 public class Simulation
    {
        public bool[] curState;
		bool[] newState;
        public Simulation()
        {
            curState = new bool[$];
			newState = new bool[$];
        }
        public void Step()
        {
		
            //bool[] newState = new bool[];
			
			$
			newState.CopyTo(curState, 0);
        }
        public void Set(int num, bool b)
        {
            curState[num] = b;
        }
        public bool Get(int num)
        {
            return curState[num];
        }
    }
}