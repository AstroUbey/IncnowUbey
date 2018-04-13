using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Xml.Linq;
/* Name:    Ubaid Malik
 * Data:    4/12/2018
 * Project: INCNOW_ContactsVs.OFAC's_SDNList
 * Status:  Final Version!
 */
namespace INCNOWPROJECT
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create two datatable objects...
            DataTable dt_SDN_Individuals = new DataTable();//This will hold data for individuals from SDN.xml
            dt_SDN_Individuals.Columns.Add("id", typeof(string));
            dt_SDN_Individuals.Columns.Add("firstName", typeof(string));
            dt_SDN_Individuals.Columns.Add("lastName", typeof(string));
            dt_SDN_Individuals.Columns.Add("placeOfBirth", typeof(string));

            DataTable dt_SDN_Companies = new DataTable();//This will hold data for companies from SDN.xml
            dt_SDN_Companies.Columns.Add("id", typeof(string));
            dt_SDN_Companies.Columns.Add("company", typeof(string));
            dt_SDN_Companies.Columns.Add("address", typeof(string));

            //Load the SDN.xml file from the web
            string url = "https://www.treasury.gov/ofac/downloads/sdn.xml";
            XNamespace ns = "http://tempuri.org/sdnList.xsd";
            XElement xelement = XElement.Load(url);
            IEnumerable<XElement> records = xelement.Descendants(ns + "sdnEntry");

            //This loop will insert all the data into the DataTable Object...
            foreach (var record in records)//All the records in the SDN.xml file read them one by one
            {
                var nameEl = record.Element(ns + "firstName");
                var countryEl1 = record.Element(ns + "placeOfBirthList");
                //For records with element value "Individual" are selected
                if (record.Element(ns + "sdnType").Value.ToUpper().Contains("Individual".ToUpper()))
                {
                    if (nameEl != null && countryEl1 != null)//if both first name and country are not null then input that row
                    {
                        dt_SDN_Individuals.Rows.Add(record.Element(ns + "uid").Value.Trim(),
                                                    record.Element(ns + "firstName").Value.Trim().ToUpper(),
                                                    record.Element(ns + "lastName").Value.Trim().ToUpper(),
                                                    record.Element(ns + "placeOfBirthList").Value.Substring
                                                    (0, record.Element(ns + "placeOfBirthList").Value.Length - 4).Trim().ToUpper());
                    }
                    else if (nameEl != null)//Otherwise if first name is not null then input that row
                    {
                        dt_SDN_Individuals.Rows.Add(record.Element(ns + "uid").Value.Trim(),
                                                    record.Element(ns + "firstName").Value.Trim().ToUpper(),
                                                    record.Element(ns + "lastName").Value.Trim().ToUpper(),
                                                    null);
                    }
                }//For records with element value "Individual" are NOT selected
                else if (!record.Element(ns + "sdnType").Value.ToUpper().Contains("Individual".ToUpper()))
                {
                    var foundEl3 = record.Element(ns + "addressList");
                    if (foundEl3 != null)//If addressList is not null then input that row
                    {
                        dt_SDN_Companies.Rows.Add(record.Element(ns + "uid").Value,
                                                  record.Element(ns + "lastName").Value.Trim().ToUpper(),
                                                  record.Element(ns + "addressList").Value.Trim().ToUpper());
                    }
                    else //Otherwise just the company name 
                    {
                        dt_SDN_Companies.Rows.Add(record.Element(ns + "uid").Value.Trim().ToUpper(),
                                                  record.Element(ns + "lastName").Value.Trim().ToUpper());
                    }
                }
            }//End Foreach loop
             //The process of collecting data from the SDN file is complete...
            Console.WriteLine("SDN file's data retrieved and stored.");

            //Now collect the data from Incnow's Contact file...
            string path = "/Users/iubey/Projects/INCNOWPROJECT/INCNOWPROJECT/PossibleSdnList-Updated.csv";
            string[] incColumnLabel = { "Company", "First", "Last", "Country" };
            DataTable incNow = GetFileIntoDataTable(path, ',', incColumnLabel);//Calling a local method...
            Console.WriteLine("Incnow file's data retrieved and stored.");

            //Create dataTable object that will hold contacts that match with SDN file...
            DataTable incNowUpdatedBasedPpl = new DataTable();
            string[] incColumnLabel1 = { "Company", "First", "Last", "Country", "Score", "Hits", "SdnId" };
            foreach (string value in incColumnLabel1)//Loop to add columns
                incNowUpdatedBasedPpl.Columns.Add(value, typeof(string));
            incNowUpdatedBasedPpl.Rows.Add(incColumnLabel1);

            //This loop check to see if we have matching data elements in both datasets
            for (int incRow = 0; incRow < incNow.Rows.Count; incRow++)
            {
                string Hits = null;//This variable will hold strings of matching hit types
                string uid1 = "", uid2 = "", uid3 = "", uid4 = "", uid5 = "", uid6 = "", uid7 = "", uid8 = "";//Will hold SDN uid

                //This loop will check to see if the given contact is a match for any individuals from SDN...
                for (int sdnRow = 0; sdnRow < dt_SDN_Individuals.Rows.Count; sdnRow++)
                {
                    string[] split = null;//This default array will be used if the last name is more than a string literal!!!
                    if (incNow.Rows[incRow]["First"].Equals(dt_SDN_Individuals.Rows[sdnRow]["firstName"]))
                    {
                        if (incNow.Rows[incRow]["Last"].ToString().Equals(dt_SDN_Individuals.Rows[sdnRow]["lastName"].ToString()))
                        {
                            if (incNow.Rows[incRow]["Country"].ToString().Contains(dt_SDN_Individuals.Rows[sdnRow]["placeOfBirth"].ToString()))
                            {
                                Hits += "Strong";//This is a Strong match!!!
                                uid1 += dt_SDN_Individuals.Rows[sdnRow]["id"] + "-";
                            }
                            else
                            {
                                Hits += "First&Last";//This is High match!!!
                                uid2 += dt_SDN_Individuals.Rows[sdnRow]["id"] + "-";
                            }
                        }
                        else
                        {
                            if (dt_SDN_Individuals.Rows[sdnRow]["lastName"].ToString().Contains(" "))
                            {
                                split = dt_SDN_Individuals.Rows[sdnRow]["lastName"].ToString().Split(" ");
                                for (int i = 0; i < split.Length; i++)
                                {
                                    if (incNow.Rows[incRow]["Last"].Equals(split[i].Trim()))
                                    {
                                        Hits += "PartialLast";//This is a partial match!!!
                                        uid6 += dt_SDN_Individuals.Rows[sdnRow]["id"] + "-";
                                    }
                                }
                            }
                            Hits += "FirstName";//This is a Low match!!!
                            uid3 += dt_SDN_Individuals.Rows[sdnRow]["id"] + "-";
                        }
                    }//End if Outer
                    else if (incNow.Rows[incRow]["Last"].ToString().Equals(dt_SDN_Individuals.Rows[sdnRow]["lastName"].ToString()) &&
                            Hits == null)
                    {
                        if (incNow.Rows[incRow]["Country"].ToString().Contains(dt_SDN_Individuals.Rows[sdnRow]["placeOfBirth"].ToString()))
                        {
                            Hits += "Last&Country";//This is a Weak match!!!
                            uid4 += dt_SDN_Individuals.Rows[sdnRow]["id"] + "-";
                        }
                        else
                        {
                            if (dt_SDN_Individuals.Rows[sdnRow]["lastName"].ToString().Contains(" "))
                            {
                                split = dt_SDN_Individuals.Rows[sdnRow]["lastName"].ToString().Split(" ");
                                for (int i = 0; i < split.Length; i++)
                                {
                                    if (incNow.Rows[incRow]["Last"].ToString().Equals(split[i].Trim()))
                                    {
                                        Hits += "PartialLast";//This is a Low match!!!
                                        uid6 += dt_SDN_Individuals.Rows[sdnRow]["id"] + "-";
                                    }
                                }
                            }
                            Hits += "LastName";//This is a Low match!!!
                            uid5 += dt_SDN_Individuals.Rows[sdnRow]["id"] + "-";
                        }
                    }//End else-if Outer
                }//End Inner loop for individuals
                 //This loop will check to see if the given contact is a match for any companies from SDN...
                for (int sdnRow = 0; sdnRow < dt_SDN_Companies.Rows.Count; sdnRow++)
                {
                    //First work to removing different types of companies that exist...
                    string holdVal = incNow.Rows[incRow]["Company"].ToString();
                    string SdnVal = dt_SDN_Companies.Rows[sdnRow]["company"].ToString();
                    if (holdVal.Contains("LLC"))
                    {
                        holdVal = holdVal.Substring(0, holdVal.Length - 4);
                        if (SdnVal.Contains("LLC"))
                            SdnVal = SdnVal.Substring(0, SdnVal.Length - 4);
                    }
                    else if (holdVal.Contains("INC."))
                    {
                        holdVal = holdVal.Substring(0, holdVal.Length - 5);
                        if (SdnVal.Contains("INC."))
                            SdnVal = SdnVal.Substring(0, SdnVal.Length - 5);
                    }
                    else if (holdVal.Contains("CORP."))
                    {
                        holdVal = holdVal.Substring(0, holdVal.Length - 6);
                        if (SdnVal.Contains("CORP."))
                            SdnVal = SdnVal.Substring(0, SdnVal.Length - 6);
                    }
                    else if (holdVal.Contains("CO."))
                    {
                        holdVal = holdVal.Substring(0, holdVal.Length - 4);
                        if (SdnVal.Contains("CO."))
                            SdnVal = SdnVal.Substring(0, SdnVal.Length - 4);
                    }
                    //Now chech if the company names are a match...
                    if (SdnVal.Equals(holdVal))
                    {
                        if (Hits != null)//This comes in handy when the contact has hits from the individual's for-loop above
                            Hits = null;//Then reset the variable

                        if (dt_SDN_Companies.Rows[sdnRow]["address"].ToString().Contains(incNow.Rows[incRow]["Country"].ToString()))
                        {
                            Hits += "Company&Country";//This is a Strong match!!!
                            uid7 += dt_SDN_Companies.Rows[sdnRow]["id"] + "-";
                        }
                        else
                        {
                            Hits += "CompanyName";//This is a weak match!!!
                            uid8 += dt_SDN_Companies.Rows[sdnRow]["id"].ToString();
                        }
                    }
                }//End Inner loop for companies

                if (Hits != null)//If hits is not empty string then insert that row into DataTable object
                    incNowUpdatedBasedPpl.Rows.Add(incNow.Rows[incRow]["Company"], incNow.Rows[incRow]["First"],
                                                   incNow.Rows[incRow]["Last"], incNow.Rows[incRow]["Country"],
                                                   GetScore(Hits), GetHits(GetScore(Hits)),
                                                   uid1 + uid2 + uid3 + uid4 + uid5 + uid6 + uid7 + uid8);

            }//End Outer loop
            Console.WriteLine("Dataset comparison complete...");
            //Create Dataview object for sorting that datatable...
            DataView view = incNowUpdatedBasedPpl.DefaultView;
            view.Sort = "Score DESC";
            incNowUpdatedBasedPpl = view.ToTable();

            //Create dataTable object that will be used for outputing the final version of the data...
            DataTable incNowUpdatedFinal = new DataTable();
            string[] incColumnLabel2 = { "Company", "First", "Last", "Country", "Hits", "SdnId" };
            foreach (string value in incColumnLabel1)//Loop to add columns
                incNowUpdatedFinal.Columns.Add(value, typeof(string));

            foreach (DataRow row in incNowUpdatedBasedPpl.Rows)//This loop will insert columns of interest
                incNowUpdatedFinal.Rows.Add(row["Company"], row["First"], row["Last"], row["Country"], row["Hits"], row["SdnId"]);


            //DisplayTable(dt_SDN_Companies); //This will output the contents of the DataTable object on the console
            DataTableToFile("IncNowContactUpdate0.csv", incNowUpdatedFinal); //This will output the contents into a file
            Console.WriteLine("Data outputed to the file complete!");
        }//End Main Method

        /* This method will generate scores for a given hit of a given record from contacts.
         * @param will need a string of hits.
         * @return will give an integer value based on hit types.
         */
        public static int GetScore(string hit)
        {
            int realScore = 0;
            //This if structure is for individual hits
            if (hit.Contains("Strong"))
                realScore += 4;
            else if (hit.Contains("First&Last"))
                realScore += 3;
            else if (hit.Contains("Last&Country"))
                realScore += 2;
            else
            {
                if (hit.Contains("FirstName"))
                {
                    realScore++;
                    if (hit.Contains("Partial"))
                        realScore += 2;
                }
                if (hit.Contains("LastName"))
                    realScore++;
            }

            //These if statements are for company hits
            if (hit.Contains("Company&Country"))
                realScore = 6;
            if (hit.Contains("CompanyName"))
                realScore = 5;

            return realScore;
        }

        /* This method will generate strings based on the scores given by getScore method.
         * @param will need int values.
         * @return will give strings based on int values.
         */
        public static string GetHits(int realScore)
        {
            string val = null;

            if (realScore == 6)
                val = "High Company Match";
            if (realScore == 5)
                val = "Low Company Match";
            if (realScore == 4)
                val = "Strong Individual Match";
            if (realScore == 3)
                val = "High Individual Match";
            if (realScore == 2)
                val = "Partical Individual Match";
            if (realScore == 1)
                val = "Low Individual Match";

            return val;
        }

        /*  This method will convert imported file data into a DataTable object.
         *  @param will need a path string for file location, delimiter for breaking rows of input,
         *  and an array of strings for column names.
         *  @return will give a DataTable object containing file data.
         */
        public static DataTable GetFileIntoDataTable(string path, char delimiter, string[] columnValue_ar)
        {//Pass the file path to the StreamReader constructor
            StreamReader sr = new StreamReader(path);
            //Create dataTable object
            DataTable dt = new DataTable();
            //Loop to add columns
            foreach (string value in columnValue_ar)
            {
                dt.Columns.Add(value, typeof(string));
            }
            string val = sr.ReadLine();
            //Continue to read until you reach end of file
            while (val != null) //Loop to add rows
            {
                string[] split = val.ToUpper().Split(delimiter);
                dt.Rows.Add(split);//Add row to the table
                val = sr.ReadLine();//Read the next line
            }
            sr.Close();//close the file

            //Format the dataset...
            for (int r = 0; r < dt.Rows.Count; r++)//Making the dataset clean...
            {
                dt.Rows[r][0] = dt.Rows[r][0].ToString().Trim().ToUpper();
                dt.Rows[r][1] = dt.Rows[r][1].ToString().Remove(dt.Rows[r][1].ToString().Length - 1).Substring(1).Trim().ToUpper();
                dt.Rows[r][2] = dt.Rows[r][2].ToString().Remove(dt.Rows[r][2].ToString().Length - 1).Substring(1).Trim().ToUpper();
                dt.Rows[r][3] = dt.Rows[r][3].ToString().Remove(dt.Rows[r][3].ToString().Length - 1).Substring(1).Trim().ToUpper();
            }

            return dt;
        }//End Method

        /*  This method will export DataTable objects into a file.
         *  @param will need a string for file name and a object of DataTable.
         */
        public static void DataTableToFile(string fileName, DataTable table)
        {
            FileStream fs = new FileStream(fileName, FileMode.CreateNew);
            StreamWriter writer = new StreamWriter(fs);
            foreach (DataRow row in table.Rows)
            {
                foreach (object item in row.ItemArray)
                {
                    writer.Write(item + " , ");
                }
                writer.WriteLine();
            }
            writer.Close();//Close the file
        }//End Method

        /*  This method will display all the contents of a DataTable object.
         */
        public static void DisplayTable(DataTable table)
        {
            for (int r = 0; r < table.Rows.Count; r++)
            {
                for (int c = 0; c < table.Columns.Count; c++)
                {
                    Console.Write(table.Rows[r][c]);
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            Console.WriteLine("This is the end of the Data Table!");
            Console.WriteLine(table.Rows.Count);
        }//End Method

    }//End Class
}//End Name Space