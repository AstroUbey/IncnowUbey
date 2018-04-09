using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Xml.Linq;
/* Name:    Ubaid Malik
 * Data:    4/03/2018
 * Project: INCNOW_ContactsVs.OFAC's_SDNList
 * Status:  Complete!
 */

namespace INCNOWPROJECT
{
    class Program
    {
        static void Main(string[] args)
        {//Setup...
            string path = "/Users/iubey/Projects/INCNOWPROJECT/INCNOWPROJECT/PossibleSdnList-Updated.csv";
            string[] incColumnLabel = { "Company", "First", "Last", "Country" };
            DataTable incNow = GetFileIntoDataTable(path, ',', incColumnLabel);
            //Format the dataset...
            for (int r = 0; r < incNow.Rows.Count; r++)//Making the dataset clean...
            {
                incNow.Rows[r][0] = incNow.Rows[r][0].ToString().Trim().ToUpper();
                incNow.Rows[r][1] = incNow.Rows[r][1].ToString().Remove(incNow.Rows[r][1].ToString().Length - 1).Substring(1).Trim().ToUpper();
                incNow.Rows[r][2] = incNow.Rows[r][2].ToString().Remove(incNow.Rows[r][2].ToString().Length - 1).Substring(1).Trim().ToUpper();
                incNow.Rows[r][3] = incNow.Rows[r][3].ToString().Remove(incNow.Rows[r][3].ToString().Length - 1).Substring(1).Trim().ToUpper();
            }

            //Create dataTable objects...
            DataTable incNowUpdatedBasedPpl = new DataTable();
            //Loop to add columns
            string[] incColumnLabel1 = { "Company", "First", "Last", "Country", "Score", "SdnId" };
            foreach (string value in incColumnLabel1)
            {
                incNowUpdatedBasedPpl.Columns.Add(value, typeof(string));
            }
            incNowUpdatedBasedPpl.Rows.Add(incColumnLabel1);

            DataTable dt_SDN_Individuals = new DataTable();
            dt_SDN_Individuals.Columns.Add("id", typeof(string));
            dt_SDN_Individuals.Columns.Add("firstName", typeof(string));
            dt_SDN_Individuals.Columns.Add("lastName", typeof(string));
            dt_SDN_Individuals.Columns.Add("placeOfBirth", typeof(string));

            DataTable dt_SDN_Companies = new DataTable();
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
                if (record.Element(ns + "sdnType").Value.ToUpper().Contains("Individual".ToUpper()))//For records with element value "Individual" are selected
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
                }
                else if (!record.Element(ns + "sdnType").Value.ToUpper().Contains("Individual".ToUpper()))//For records with element value "Individual" are NOT selected
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
            //This loop check to see if we have matching data elements in both datasets
            for (int incRow = 0; incRow < incNow.Rows.Count; incRow++)
            {
                int score = 0;
                string uid1 = "", uid2 = "", uid3 = "", uid4 = "";
                for (int sdnRow = 0; sdnRow < dt_SDN_Individuals.Rows.Count; sdnRow++)
                {
                    if (incNow.Rows[incRow]["First"].ToString().Equals(dt_SDN_Individuals.Rows[sdnRow]["firstName"].ToString()))
                    {
                        if (incNow.Rows[incRow]["Last"].ToString().Equals(dt_SDN_Individuals.Rows[sdnRow]["lastName"].ToString()))
                        {
                            if (incNow.Rows[incRow]["Country"].ToString().Contains(dt_SDN_Individuals.Rows[sdnRow]["placeOfBirth"].ToString()))
                            {
                                score++;//This is a Strong match!!!
                                uid1 = dt_SDN_Individuals.Rows[sdnRow]["id"].ToString();
                            }

                        }
                    }
                    else if (incNow.Rows[incRow]["Last"].ToString().Equals(dt_SDN_Individuals.Rows[sdnRow]["lastName"].ToString()))
                    {
                        if (incNow.Rows[incRow]["Country"].ToString().Contains(dt_SDN_Individuals.Rows[sdnRow]["placeOfBirth"].ToString()))
                        {
                            score++;//This is a weak match!!!
                            uid2 = dt_SDN_Individuals.Rows[sdnRow]["id"].ToString();
                        }
                    }
                    else if (incNow.Rows[incRow]["First"].ToString().Equals(dt_SDN_Individuals.Rows[sdnRow]["firstName"].ToString()))
                    {
                        if (incNow.Rows[incRow]["Last"].ToString().Equals(dt_SDN_Individuals.Rows[sdnRow]["lastName"].ToString()))
                        {
                            score++;//This is a okay match!!!
                            uid3 = dt_SDN_Individuals.Rows[sdnRow]["id"].ToString();
                        }
                    }
                }//End Inner loop
                //This condition is for checking Companies with similar countries
                for (int sdnRow = 0; sdnRow < dt_SDN_Companies.Rows.Count; sdnRow++)
                {
                    if (incNow.Rows[incRow]["Company"].ToString().Equals(dt_SDN_Companies.Rows[sdnRow]["company"].ToString()))
                    {
                        if (incNow.Rows[incRow]["Country"].ToString().Contains(dt_SDN_Companies.Rows[sdnRow]["address"].ToString()))
                        {
                            score++;//This is a weak match!!!
                            uid4 = dt_SDN_Companies.Rows[sdnRow]["id"].ToString();
                        }
                    }
                }
                if(score>0)//If score is not zero then insert that row into DataTable object
                    incNowUpdatedBasedPpl.Rows.Add(incNow.Rows[incRow]["Company"], incNow.Rows[incRow]["First"], incNow.Rows[incRow]["Last"],
                                       incNow.Rows[incRow]["Country"], score, uid1 + uid2 + uid3 + uid4);
            }//End Outer loop

            DisplayTable(incNow); //This will output the contents of the DataTable object on the console
            DataTableToFile("IncNowContactUpdate2.csv", incNowUpdatedBasedPpl); //This will output the contents into a file

        }//End Main Method

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





//Junk Code!!!

/*
 * This method will update the Score attribute.
 * This is method considers three dataTables objects, 
 * can be of any size (only table_sdn_ppl has to be greater in column length then other two tabels),
 * current setup considers starting positions
 *
public static void UpdatedScore(DataTable table_inc, DataTable table_sdn_ppl, DataTable table_sdn_com)
{
    int count_score = 0;
    for (int ithNow_R = 0; ithNow_R < table_inc.Columns.Count; ithNow_R++)
    {//Pick a row from Inc
        DataRow inc_row = table_inc.Rows[ithNow_R];
        string[] inc_element_ar = inc_row.ToString().ToUpper().Split(',');
        //inc_element_ar = inc_element.ToString().ToUpper().Trim();
        Console.WriteLine("--->" + inc_element_ar.ToString());
        for (int ithPpl_R = 0; ithPpl_R < table_sdn_ppl.Columns.Count; ithPpl_R++)//Outer-loop
        {
            DataRow ppl_row = table_sdn_ppl.Rows[ithPpl_R];
            string[] ppl_element_ar = ppl_row.ToString().ToUpper().Split(' ');
            //ppl_element_ar = ppl_element.ToString().ToUpper().Trim();

            for (int i = 1; i < ppl_element_ar.Length; i++)//double-Inner-loop
            {
                Console.WriteLine("-->" + inc_element_ar[i].Trim() + "----->" + ppl_element_ar[i].Trim() + "<---");
                if (inc_element_ar[i].Trim().Contains(ppl_element_ar[i].Trim()))
                {
                    count_score++;//increment when true
                }
            }//End-deeper-loop

            DataRow company_row = table_sdn_com.Rows[ithPpl_R];
            string[] company_element_ar = company_row.ToString().ToUpper().Split(' ');
            //company_element_ar = ppl_element.ToString().ToUpper().Trim();
            /*
            for (int i = -1; i < company_element_ar.Length; i++)//double-Inner-loop
            {
                if (inc_element_ar[i].Trim().Contains(company_element_ar[1].Trim()))
                {
                    count_score++;//increment when true
                }
            }//End-deeper-loop
            inc_row.ItemArray[4] = count_score;
            count_score = 0;//Reset the variable before ending loop
        }//End Inner-loop
    }//End Outer-loop
}//End Method
 */