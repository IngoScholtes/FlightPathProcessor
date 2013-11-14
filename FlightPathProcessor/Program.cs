using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightPathProcessor
{
    struct trip
    {
        public bool valid;
        public string itinerary;
        public string seq;
        public string coupons;
        public string origin;
        public string dest;
    }

    class Program
    {
        static void Main(string[] args)
        {            
            string[] trip_lines = System.IO.File.ReadAllLines(args[0]);

            List<trip> trips = new List<trip>(trip_lines.Length);

            for(int i =1; i<trip_lines.Length; i++)
                {
                    trip t = fromLine(trip_lines[i]);
                    if (t.valid)
                        trips.Add(t);
                }

            Console.WriteLine("Collected {0} trips", trips.Count);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("time,source,target");

            var ordered_seq = trips.OrderBy(x => x.itinerary + x.seq);
            string currentitin = ordered_seq.ElementAt(0).itinerary;

            Console.WriteLine("Reordered data.");

            int time = 0;

            foreach (trip t in ordered_seq)
            {
                if (t.itinerary == currentitin)
                    time++;
                else
                {
                    currentitin = t.itinerary;
                    time += 2;
                }
                sb.AppendLine(string.Format("{0},{1},{2}", time, t.origin, t.dest));
            }

            System.IO.File.WriteAllText("..\\ordered.csv", sb.ToString());

            Console.WriteLine("Wrote ordered sequence");
        }

        static trip fromLine(string line)
        {
            string[] split = line.Split(',');
            trip c = new trip();
            c.valid = false;
            c.itinerary = split[0];
            c.seq = split[1];
            c.coupons = split[2];
            c.origin = split[3];
            c.dest = split[4];

            if (c.coupons != "1")
                c.valid = true;
            return c;
        }
    }
}
