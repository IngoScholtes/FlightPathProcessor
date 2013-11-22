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
        public bool collect;
        public string itinerary;
        public string seq;
        public string coupons;
        public string origin;
        public string dest;
        public string tkCarrier;
        public string opCarrier;
        public string rpCarrier;
    }

    class Program
    {
        static Dictionary<string, int> fields = new Dictionary<string, int>();
        static int time = 0;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: FlightPathProcessor source_file dest_file carrierID");
            }

            List<trip> trips = new List<trip>(10000);

            System.IO.StreamReader sr = new System.IO.StreamReader(args[0]);
            string carrier_to_collect = args[2];

            string[] header = sr.ReadLine().Split(',');

            for (int i = 0; i < header.Length; i++)
                fields[header[i].Replace("\"","")] = i;

            do
            {
                string line = sr.ReadLine();
                trip t = fromLine(line);
                if (t.collect && t.tkCarrier == carrier_to_collect)
                    trips.Add(t);

            } while (!sr.EndOfStream);
            
            Console.WriteLine("Collected {0} trips", trips.Count);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("time,source,target");

            var ordered_seq = trips.OrderBy(x => x.itinerary + "_" + x.seq);
            string currentitin = ordered_seq.ElementAt(0).itinerary;

            Console.WriteLine("Reordered data.");                     

            Dictionary<int, trip> current_itinerary = new Dictionary<int, trip>();

            foreach (trip t in ordered_seq)
            {
                // This is the first coupon of a new itinerary
                if (t.itinerary != currentitin)                                                
                {
                    // Write the previous list of coupons                 
                   sb.Append(tripsToString(current_itinerary));
                   current_itinerary.Clear();
                   currentitin = t.itinerary;
                }
                current_itinerary[int.Parse(t.seq)] = t;
            }

            System.IO.File.WriteAllText(args[1], sb.ToString());

            Console.WriteLine("Wrote ordered sequence");
        }

        static string tripsToString(Dictionary<int, trip> trips)
        {
            if (trips.Count == 0)
                return "";
            string val = "";
            for (int seq = 1; seq <= trips.Count; seq++)
            {
                // Filter out itineraries with missing coupons
                if (!trips.ContainsKey(seq))
                    return "";
                if (trips[seq].coupons != trips.Count.ToString())
                    return "";
                // Filter out return trips
                if (seq > 1 && trips[seq-1].origin == trips[seq].dest)
                    return "";

                val = val + string.Format("{0},{1},{2}\n", time++, trips[seq].origin, trips[seq].dest);
            }
            time++;
            return val;
        }

        static trip fromLine(string line)
        {
            string[] split = line.Split(',');
            trip c = new trip();
            c.collect = false;
            c.itinerary = split[fields["ItinID"]].Replace("\"", "");
            c.seq = split[fields["SeqNum"]].Replace("\"", "");
            c.coupons = split[fields["Coupons"]].Replace("\"", "");
            c.origin = split[fields["Origin"]].Replace("\"", "");
            c.dest = split[fields["Dest"]].Replace("\"", "");
            c.opCarrier = split[fields["OpCarrier"]].Replace("\"", "");
            c.tkCarrier = split[fields["TkCarrier"]].Replace("\"", "");
            c.rpCarrier = split[fields["RPCarrier"]].Replace("\"", "");

            if (c.coupons != "1")
                c.collect = true;
            return c;
        }
    }
}
