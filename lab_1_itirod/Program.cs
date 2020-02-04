using OfficeOpenXml;
using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Collections.Generic;

namespace lab_1_itirod
{
    class Program
    {
        /// <summary>
        /// main function 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.Unicode;


            //args = new string[] { "-i", "c", "-o", "out", "-f", "JSON" };
            string inputFile = "", outputFile = "", format = "";

            try
            {
                for (int i = 0; i < args.Length; i += 2) 
                    switch (args[i])
                    {
                        case "-i":
                            inputFile = args[i + 1];
                            if (GetExtension(inputFile) != "csv") throw new Exception("Input file format error");
                            break;
                        case "-o":
                            outputFile = args[i + 1];
                            break;
                        case "-f":
                            if (args[i + 1] != "Excel" && args[i + 1] != "JSON") throw new Exception("Output file format error");
                            else format = args[i + 1];
                            break;
                        default:
                            throw new Exception("Unknown flag");
                    }
                if (inputFile == "" || outputFile == "") throw new Exception("Need input/output file name");
                if (format == "Excel") ProcessExcel(inputFile, outputFile);
                else ProcessJSON(inputFile, outputFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не возможно выполнить задание по причине:");
                LogException(ex);
                Console.WriteLine("Введите значения в ручную:\n введите исходный файл\n");
                inputFile = Console.ReadLine();
                Console.WriteLine("введите целевой файл\n");
                outputFile = Console.ReadLine();
                Console.WriteLine("введите формат файла (Excel / JSON\n");
                format = Console.ReadLine();

                if (format == "Excel") ProcessExcel(inputFile, outputFile);
                else ProcessJSON(inputFile, outputFile);

            }

        }
        /// <summary>
        /// формирование нового файла в формате .xlsx из исходного
        /// </summary>
        /// <param name="inputFile">файл с исходными данными</param>
        /// <param name="outputFile">выходной файл</param>
        public static void ProcessExcel(string inputFile, string outputFile)
        {
            try
            {
                var fi = new FileInfo(inputFile);
                if (!fi.Exists) throw new Exception("Error. File not found.");
                string[] fileData = File.ReadAllLines(inputFile);
                int n = fileData.Length;

                var dataTable = new DataTable("data");
                string[] temp = fileData[0].Split(";");
                int m = temp.Length;
                for (int i = 0; i < 3; i++)
                    dataTable.Columns.Add(new DataColumn(temp[i], typeof(string)));
                for (int i = 3; i < m; i++)
                    dataTable.Columns.Add(new DataColumn(temp[i], typeof(float)));
                dataTable.Columns.Add(new DataColumn("average per student", typeof(float)));

                float[] subjects = new float[m - 3];

                var row = dataTable.NewRow();

                for (int i = 1; i < n; i++)
                {
                    temp = fileData[i].Split(";");
                    row[0] = temp[0];
                    row[1] = temp[1];
                    row[2] = temp[2];
                    float sum = 0;
                    for (int j = 3; j < m; j++)
                    {
                        row[j] = temp[j] == "" ? "0" : temp[j];
                        sum += Convert.ToInt32(temp[j] == "" ? "0" : temp[j]);
                        subjects[j - 3] += Convert.ToInt32(temp[j] == "" ? "0" : temp[j]);
                    }
                    row[m] = sum / (m - 3);
                    dataTable.Rows.Add(row);
                    row = dataTable.NewRow();
                }

                row[0] = "average per subject";
                for (int i = 3; i < m; i++)
                {
                    row[i] = subjects[i - 3] / (n - 1);
                }
                dataTable.Rows.Add(row);

                fi = new FileInfo(outputFile + ".xlsx");
                if (fi.Exists) fi.Delete();

                using (var pck = new ExcelPackage(fi))
                {
                    var worksheet = pck.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells.LoadFromDataTable(dataTable, true);
                    pck.Save();
                }
                Console.WriteLine("Completed");
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        /// <summary>
        /// формирование нового файла в формате .json из исходного
        /// </summary>
        /// <param name="inputFile">файл с исходными данными</param>
        /// <param name="outputFile">выходной файл</param>
        public static void ProcessJSON(string inputFile, string outputFile)
        {
            try
            {
                var fi = new FileInfo(inputFile);
                if (!fi.Exists) throw new Exception("Error. File not found.");
                string[] fileData = File.ReadAllLines(inputFile);
                int n = fileData.Length;

                List<Student> students = new List<Student>();
                List<string> subjects = new List<string>();
                subjects.AddRange(fileData[0].Split(";"));
                subjects.RemoveRange(0, 3);

                for (int i = 1; i < n; i++)
                {
                    string[] temp = fileData[i].Split(";");
                    Student student = new Student(temp[0], temp[1], temp[2]);
                    for (int j = 3; j < temp.Length; j++)
                        student.Marks.Add(new Subject(subjects[j - 3], Convert.ToInt32(temp[j] == "" ? "0" : temp[j])));
                    student.CalcAvg();
                    students.Add(student);
                }

                List<Subject> AvgMarks = new List<Subject>();
                for (int i = 0; i < subjects.Count; i++)
                {
                    float avg = 0;
                    for (int j = 0; j < students.Count; j++)
                        avg += students[j].Marks[i].Mark;
                    avg /= subjects.Count;
                    AvgMarks.Add(new Subject(students[0].Marks[i].Name, avg));
                }
                using (FileStream fs = new FileStream(outputFile + ".json", FileMode.Create))
                {
                    DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(List<Student>));
                    jsonFormatter.WriteObject(fs, students);
                    jsonFormatter = new DataContractJsonSerializer(typeof(List<Subject>));
                    jsonFormatter.WriteObject(fs, AvgMarks);
                }
                Console.WriteLine("Completed");
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        /// <summary>
        /// функция возвращает имя файла без расширения
        /// </summary>
        /// <param name="fileName">имя файла</param>
        /// <returns></returns>
        public static string GetExtension(string fileName)
        {
            string[] temp = fileName.Split(".");
            return temp[temp.Length - 1];
        }
        /// <summary>
        /// функция для записи ошибок в текстовый файл
        /// </summary>
        /// <param name="ex">текст ошибки</param>
        public static void LogException(Exception ex)
        {
            using (StreamWriter sw = new StreamWriter((DateTime.Now.ToString("s") + ".txt").Replace(':', '-'), false, Encoding.Default))
            {
                sw.WriteLine(ex.ToString());
            }
            Console.WriteLine("Error. See logs");
        }
    }
}

