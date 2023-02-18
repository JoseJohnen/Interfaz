﻿using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Interfaz.Utilities
{
    public class UtilityAssistant
    {
        /// <summary>
        /// Compare Vector3s, usually used in position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of the imaginary nature of the answer all compacted in a Vector3
        /// </summary>
        /// <param name="ValueA"></param>
        /// <param name="ValueB"></param>
        /// <returns>Returns a Vector3 with the result of each directional difference (1,0,-1) between ValueA and ValueB on each of his axis independantly</returns>
        public static Vector3 DistanceModifierByVectorComparison(Vector3 ValueA, Vector3 ValueB)
        {
            Vector3 result = Vector3.Zero;
            result.X = DistanceModifierByAxis(ValueA.X, ValueB.X);
            result.Y = DistanceModifierByAxis(ValueA.Y, ValueB.Y);
            result.Z = DistanceModifierByAxis(ValueA.Z, ValueB.Z);
            return result;
        }

        /// <summary>
        /// Compare Vector3s, usually used in position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of his distance to 0, all compacted in a Vector3.
        /// </summary>
        /// <param name="ValueA"></param>
        /// <param name="ValueB"></param>
        /// <returns>Returns a Vector3 with the result of each directional cartesian difference (1,0,-1) between ValueA and ValueB on each of his axis independantly</returns>
        public static Vector3 DistanceModifierByCartesianVectorComparison(Vector3 ValueA, Vector3 ValueB)
        {
            Vector3 result = Vector3.Zero;
            result.X = DistanceModifierByCartesianAxis(ValueA.X, ValueB.X);
            result.Y = DistanceModifierByCartesianAxis(ValueA.Y, ValueB.Y);
            result.Z = DistanceModifierByCartesianAxis(ValueA.Z, ValueB.Z);
            return result;
        }

        /// <summary>
        /// Compare floats, usually used in floats of position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of the cartesian distance (i.e. which one is closer or farther to 0) 
        /// </summary>
        /// <param name="ValueA">a flota</param>
        /// <param name="ValueB">another float to make the comparison with</param>
        /// <returns>returns 1 if ValueA is farther to 0 or -1 if ValueB is farther to 0, if they are equal it will return 0</returns>
        public static float DistanceModifierByCartesianAxis(float ValueA, float ValueB)
        {
            try
            {
                float evaluator = 0;
                if ((ValueA < 0 && ValueB > 0) || (ValueA > 0 && ValueB < 0))
                {
                    if (ValueA > ValueB)
                    {
                        evaluator = -1;
                    }
                    else if (ValueA < ValueB)
                    {
                        evaluator = 1;
                    }
                    return evaluator;
                }

                float a, b, c;
                //Determinar si números de mismo signo son positivos o negativos
                if (ValueA > 0)
                {
                    c = 1;
                }
                else if (ValueA < 0)
                {
                    c = -1;
                }
                else
                {
                    if (ValueB > 0)
                    {
                        c = 1;
                    }
                    else if (ValueB < 0)
                    {
                        c = -1;
                    }
                    else
                    {
                        //Si llega acá es porque ambos números son 0
                        return 0;
                    }
                }

                //Si son de igual signo y no son 0 ambos
                a = ValueA < 0 ? ValueA * -1 : ValueA;
                b = ValueB < 0 ? ValueB * -1 : ValueB;

                if (c > 0)
                {
                    if (a > b)
                    {
                        evaluator = 1;
                    }
                    else if (a < b)
                    {
                        evaluator = -1;
                    }
                }
                else if (c < 0)
                {
                    if (a > b)
                    {
                        evaluator = -1;
                    }
                    else if (a < b)
                    {
                        evaluator = 1;
                    }
                }

                return evaluator;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceModifierByCartesianAxis(): " + ex.Message);
                return 0;
            }
        }

        public static string ExtractValues(string instructions, string particle, out string part1, out string part2)
        {
            try
            {
                if (!instructions.Contains(particle))
                {
                    part1 = String.Empty;
                    part2 = String.Empty;
                    return instructions;
                }

                //Extract relevant part
                string particleswithdots = particle + ":";
                string b = instructions.Substring(instructions.IndexOf(particle));
                string d = b.Contains("r/n/") ? b.Substring(0, b.IndexOf("r/n/")) : b;

                //Process relevant part
                string specificRelevantInstruction = d.Substring(particleswithdots.Length);
                part1 = specificRelevantInstruction.Substring(0, 2);
                part2 = specificRelevantInstruction.Substring(2);
                return specificRelevantInstruction;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string, out string, out string): " + ex.Message);
                part1 = String.Empty;
                part2 = String.Empty;
                return String.Empty;
            }
        }

        public static string ExtractValues(string instructions, string particle, out string part1, out string part2, out string part3)
        {
            try
            {
                if (!instructions.Contains(particle))
                {
                    part1 = String.Empty;
                    part2 = String.Empty;
                    part3 = String.Empty;
                    return instructions;
                }

                //Extract relevant part
                string particleswithdots = particle + ":";
                string b = instructions.Substring(instructions.IndexOf(particle));
                string d = b.Contains("r/n/") ? b.Substring(0, b.IndexOf("r/n/")) : b;

                //Process relevant part
                string specificRelevantInstruction = d.Substring(particleswithdots.Length);
                part1 = specificRelevantInstruction.Substring(0, 2);
                part2 = specificRelevantInstruction.Substring(2, 2);
                part3 = specificRelevantInstruction.Substring(4);
                return specificRelevantInstruction;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string, out string, out string): " + ex.Message);
                part1 = String.Empty;
                part2 = String.Empty;
                part3 = String.Empty;
                return String.Empty;
            }
        }

        public static string ExtractValues(string instructions, string particle)
        {
            try
            {
                if (!instructions.Contains(particle))
                {
                    return instructions;
                }

                //Extract relevant part
                string particleswithdots = particle + ":";
                string b = instructions.Substring(instructions.IndexOf(particle));
                string d = String.Empty;
                if (b.Contains("\r\n"))
                {
                    d = b.Substring(0, b.IndexOf("\r\n"));
                }
                else
                {
                    d = b;
                }

                //Process relevant part
                string specificRelevantInstruction = d.Substring(particleswithdots.Length);
                return specificRelevantInstruction;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string): " + ex.Message);
                return String.Empty;
            }
        }

        /// <summary>
        /// Compare floats, usually used in floats of position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of the imaginary nature of the answer 
        /// </summary>
        /// <param name="ValueA">a flota</param>
        /// <param name="ValueB">another float to make the comparison with</param>
        /// <returns>returns 1 if ValueA is bigger or -1 if ValueB is bigger, if they are equal it will return 0</returns>
        public static float DistanceModifierByAxis(float ValueA, float ValueB)
        {
            try
            {
                float evaluator = 0;
                if (Math.Round(ValueA) > Math.Round(ValueB))
                {
                    evaluator = 1;
                }
                else if (Math.Round(ValueA) < Math.Round(ValueB))
                {
                    evaluator = -1;
                }
                return evaluator;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceModifierByAxis(): " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Compare floats, usually used in Vectors of position, to determine what is the distance between both numerically, it will return a positive
        /// </summary>
        /// <param name="ValueA">a float</param>
        /// <param name="ValueB">another float to make the comparison with</param>
        /// <returns>the distance between the two</returns>
        public static float DistanceComparitorByAxis(float ValueA, float ValueB)
        {
            try
            {
                float evaluator = 0;
                if (ValueA > ValueB)
                {
                    evaluator = ValueA - ValueB;
                }
                else
                {
                    evaluator = ValueB - ValueA;
                }
                return evaluator;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceComparitorByAxis(): " + ex.Message);
                return 0;
            }
        }

        public static double ConvertRadiansToDegrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }

        /// <summary>
        /// Modulates a quaternion by another.
        /// </summary>
        /// <param name="left">The first quaternion to modulate.</param>
        /// <param name="right">The second quaternion to modulate.</param>
        /// <param name="result">When the moethod completes, contains the modulated quaternion.</param>
        public static Quaternion MultiplyQuaternions(Quaternion left, Quaternion right)
        {
            float lx = left.X;
            float ly = left.Y;
            float lz = left.Z;
            float lw = left.W;
            float rx = right.X;
            float ry = right.Y;
            float rz = right.Z;
            float rw = right.W;

            Quaternion result = new Quaternion();
            result.X = (rx * lw + lx * rw + ry * lz) - (rz * ly);
            result.Y = (ry * lw + ly * rw + rz * lx) - (rx * lz);
            result.Z = (rz * lw + lz * rw + rx * ly) - (ry * lx);
            result.W = (rw * lw) - (rx * lx + ry * ly + rz * lz);
            return result;
        }

        public static Quaternion ToQuaternion(Vector3 v)
        {

            float cy = (float)Math.Cos(v.Z * 0.5);
            float sy = (float)Math.Sin(v.Z * 0.5);
            float cp = (float)Math.Cos(v.Y * 0.5);
            float sp = (float)Math.Sin(v.Y * 0.5);
            float cr = (float)Math.Cos(v.X * 0.5);
            float sr = (float)Math.Sin(v.X * 0.5);

            return new Quaternion
            {
                W = (cr * cp * cy + sr * sp * sy),
                X = (sr * cp * cy - cr * sp * sy),
                Y = (cr * sp * cy + sr * cp * sy),
                Z = (cr * cp * sy - sr * sp * cy)
            };

        }

        public static Vector3 ToEulerAngles(Quaternion q)
        {
            Vector3 angles = new();

            // roll / x
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch / y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw / z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        //Cuidado, no funciona con con objetos dentro de objetos TODO: Arreglar eso
        public static string[] CutJson(string jsonToCut)
        {
            try
            {
                string[] result = null;
                if (!string.IsNullOrWhiteSpace(jsonToCut))
                {
                    string tempString = jsonToCut.ReplaceFirst("{", "").ReplaceLast("}", "");

                    if (tempString.Contains(", "))
                    {
                        result = tempString.Split(", ");
                        int i = 0;
                        foreach (string str in result)
                        {
                            result[i] = str.Substring(str.IndexOf(":") + 1).Replace("\"", "");
                            i++;
                        }
                        return result;
                    }

                    if (tempString.Contains(" "))
                    {
                        result = tempString.Split(" ");
                        return result;
                    }
                }

                result = new string[0];
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string[] CutJson(string): " + ex.Message);
                return new string[0];
            }
        }

        //Función original
        /*public static string[] CutJson(string jsonToCut)
        {
            try
            {
                string[] result = null;
                if (!string.IsNullOrWhiteSpace(jsonToCut))
                {
                    string tempString = jsonToCut.Replace("{", "").Replace("}", "");


                    if (tempString.Contains(", "))
                    {
                        result = tempString.Split(", ");
                        int i = 0;
                        foreach (string str in result)
                        {
                            result[i] = str.Substring(str.IndexOf(":") + 1).Replace("\"", "");
                            i++;
                        }
                        return result;
                    }

                    if (tempString.Contains(" "))
                    {
                        result = tempString.Split(" ");
                        return result;
                    }
                }

                result = new string[0];
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string[] CutJson(string): " + ex.Message);
                return new string[0];
            }
        }*/

        public static T XmlToClass<T>(string xml)
        {
            string toProcess = string.Empty;
            try
            {
                toProcess = xml.Replace("xmlns:xsi=http://www.w3.org/2001/XMLSchema-instance xmlns:xsd=http://www.w3.org/2001/XMLSchema", "").Replace("version=1.0", "version=\"1.0\"").Replace("utf-16", "\"utf-16\"").Replace("UTF-8", "\"UTF-8\"");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (StringReader textReader = new StringReader(toProcess))
                {
                    if (textReader != null)
                    {
                        return (T)xmlSerializer.Deserialize(textReader);
                    }
                    else
                    {
                        Console.WriteLine("StringReader is Null: " + xml);
                    }
                }
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error T XmlToClass<T>: " + ex.Message + " Variable in processing: " + toProcess);
                return default(T);
            }
        }

        public static Quaternion RotateX(float angle)
        {
            //Determina la rotación a partir de un ángulo
            float num = angle * 0.5f;
            return new Quaternion((float)Math.Sin(num), 0f, 0f, (float)Math.Cos(num));
        }

        public static Quaternion RotateY(float angle)
        {
            //Determina la rotación a partir de un ángulo
            float num = angle * 0.5f;
            return new Quaternion(0f, (float)Math.Sin(num), 0f, (float)Math.Cos(num));
        }

        public static Quaternion RotateZ(float angle)
        {
            //Determina la rotación a partir de un ángulo
            float num = angle * 0.5f;
            return new Quaternion(0f, 0f, (float)Math.Sin(num), (float)Math.Cos(num));
        }

        public static Quaternion StringToQuaternion(string information)
        {
            try
            {
                string sQuaternion = "(" + information.Replace(" ", ",") + ")";
                // Remove the parentheses
                if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")"))
                {
                    sQuaternion = sQuaternion.Substring(1, sQuaternion.Length - 2);
                }

                // split the items
                string[] sArray = sQuaternion.Split(',');

                // store as a Vector3
                Quaternion result = new Quaternion(
                    float.Parse(sArray[0].Substring(2)),
                    float.Parse(sArray[1].Substring(2)),
                    float.Parse(sArray[2].Substring(2)),
                    float.Parse(sArray[3].Substring(2)));

                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Quaternion StringToQuaternion(string): " + ex.Message, ConsoleColor.Red);
                return Quaternion.Identity;
            }
        }

        public static string ValidateAndExtractInstructions(string instructions, string signature, out string remainingInstructions)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(instructions))
                {
                    remainingInstructions = instructions;
                    return String.Empty;
                }
                else
                {
                    if (!instructions.Contains(signature))
                    {
                        remainingInstructions = instructions;
                        return String.Empty;
                    }
                }

                string specificRelevantInstruction = UtilityAssistant.ExtractValues(instructions, signature);
                remainingInstructions = instructions.Replace(specificRelevantInstruction, "");
                return specificRelevantInstruction;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ValidateInstructions(string, string, out string): " + ex.Message);
                remainingInstructions = instructions;
                return String.Empty;
            }
        }

        public static string ValidateAndExtractInstructions(string instructions, string signature, out string remainingInstructions, out string part1, out string part2)
        {
            try
            {
                part1 = string.Empty;
                part2 = string.Empty;

                if (String.IsNullOrWhiteSpace(instructions))
                {
                    remainingInstructions = instructions;
                    return String.Empty;
                }
                else
                {
                    if (!instructions.Contains(signature))
                    {
                        remainingInstructions = instructions;
                        return String.Empty;
                    }
                }

                string specificRelevantInstruction = UtilityAssistant.ExtractValues(instructions, signature, out part1, out part2);
                remainingInstructions = instructions.Replace(specificRelevantInstruction, "");
                return specificRelevantInstruction;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ValidateInstructions(string, string, out string, out string, out string): " + ex.Message);
                remainingInstructions = instructions;
                part1 = string.Empty;
                part2 = string.Empty;
                return String.Empty;
            }
        }

        public static string ValidateAndExtractInstructions(string instructions, string signature, out string remainingInstructions, out string part1, out string part2, out string part3)
        {
            try
            {
                part1 = string.Empty;
                part2 = string.Empty;
                part3 = string.Empty;

                if (String.IsNullOrWhiteSpace(instructions))
                {
                    remainingInstructions = instructions;
                    return String.Empty;
                }
                else
                {
                    if (!instructions.Contains(signature))
                    {
                        remainingInstructions = instructions;
                        return String.Empty;
                    }
                }

                string specificRelevantInstruction = UtilityAssistant.ExtractValues(instructions, signature, out part1, out part2, out part3);
                remainingInstructions = instructions.Replace(specificRelevantInstruction, "");
                return specificRelevantInstruction;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ValidateInstructions(string, string, out string, out string, out string, out string): " + ex.Message);
                remainingInstructions = instructions;
                part1 = string.Empty;
                part2 = string.Empty;
                part3 = string.Empty;
                return String.Empty;
            }
        }

        public static string CleanJSON(string json)
        {
            string strTemp = json;
            string strSecTemp = string.Empty;
            try
            {
                //Verificar si es relevante siquiera correr la función
                if (string.IsNullOrWhiteSpace(strTemp))
                {
                    return string.Empty;
                }

                if (strTemp.IndexOf("{") > 1)
                {
                    //Console.WriteLine("Entro a indexOf>1");
                    strSecTemp = strTemp.Substring(0, strTemp.IndexOf("{"));
                    strTemp = strTemp.Replace(strSecTemp, "");
                }

                if (strTemp.Contains("}"))
                {
                    if ((strTemp.Length - strTemp.LastIndexOf("}")) > 2)
                    {
                        //Console.WriteLine("Entro a LastIndexOf>2");
                        strTemp = strTemp.Replace(strTemp.Substring(strTemp.LastIndexOf("}") + 1), "");
                    }
                }

                /*if (strTemp.Contains("\\"))
                {
                    strTemp = strTemp.Replace("\\", "");
                }*/

                while (strTemp.Contains("\\"))
                {
                    strTemp = strTemp.Replace("\\", "");
                }

                if (strTemp.Contains("u0022"))
                {
                    strTemp = strTemp.Replace("u0022", "\"");
                }

                if (strTemp.Contains("},]"))
                {
                    strTemp = strTemp.Replace("},]", "}]");
                }

                if (strTemp.Contains("u003C"))
                {
                    strTemp = strTemp.Replace("u003C", "<");
                }

                if (strTemp.Contains("u003E"))
                {
                    strTemp = strTemp.Replace("u003E", ">");
                }

                while (strTemp.Contains("\"\""))
                {
                    strTemp = strTemp.Replace("\"\"", "\"");
                }

                if (strTemp.Contains("\"{\""))
                {
                    strTemp = strTemp.Replace("\"{\"", "{\"");
                }

                if (strTemp.Contains("\"}\""))
                {
                    strTemp = strTemp.Replace("\"}\"", "\"}");
                }

                if (strTemp.Contains("},]"))
                {
                    strTemp = strTemp.Replace("},]", "}]");
                }

                /*if (strTemp.Contains("\""))
                {
                    strTemp = strTemp.Replace("\"", "");
                }*/

                //Verificar que algo quedo después de la limpieza, asumiendo que el string no era solo basura
                if (string.IsNullOrWhiteSpace(strTemp))
                {
                    return string.Empty;
                }

                //Cleaning remanents outside of "{ }" so to be certain is valid
                if (strTemp.Contains("\"{\""))
                {
                    strTemp = strTemp.Replace("\"{\"", "{\"");
                }

                if (strTemp.Contains("\"}\""))
                {
                    strTemp = strTemp.Replace("\"}\"", "\"}");
                }

                if (strTemp.Contains("}\""))
                {
                    strTemp = strTemp.Replace("}\"", "}");
                }

                Regex LRegex = new Regex(Regex.Escape("{"));
                int intLRegex = new Regex(Regex.Escape("{")).Matches(strTemp).Count;
                Regex RRegex = new Regex(Regex.Escape("}"));
                int intRRegex = new Regex(Regex.Escape("}")).Matches(strTemp).Count;
                int rslt = intRRegex + intLRegex;
                while (LRegex.Matches(strTemp).Count != RRegex.Matches(strTemp).Count)
                {
                    if (LRegex.Matches(strTemp).Count > RRegex.Matches(strTemp).Count)
                    {
                        //El primero, elimina la primera instancia, el segundo, elimina todo hasta la segunda instancia
                        if(strTemp.Contains("{"))
                        {
                            strTemp = strTemp.Substring(strTemp.IndexOf("{") + 1);
                        }
                        if (strTemp.Contains("{"))
                        {
                            strTemp = strTemp.Substring(strTemp.IndexOf("{"));
                        }
                    }

                    if (LRegex.Matches(strTemp).Count < RRegex.Matches(strTemp).Count)
                    {
                        //El primero, elimina la última instancia, el segundo, elimina todo hasta la anterior
                        //a la última instancia
                        if (strTemp.Contains("}"))
                        {
                            strTemp = strTemp.Substring(0, strTemp.LastIndexOf("}") - 1);
                        }
                        if (strTemp.Contains("}"))
                        {
                            strTemp = strTemp.Substring(0, strTemp.IndexOf("}"));
                        }
                    }

                    if (LRegex.Matches(strTemp).Count == 0 && RRegex.Matches(strTemp).Count == 0)
                    {
                        //Porque en este caso no es JSON en absoluto, pero debería serlo
                        return string.Empty;
                    }
                }

                return strTemp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CleanJSON(string): " + ex.Message);
                return strTemp;
            }
        }

        public static string Base64Encode(string plainText)
        {
            try
            {
                byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string Base64Encode(string): String: {0} | Message: {1}", plainText, ex.Message);
                return plainText;
            }
        }

        public static string Base64Decode(string base64EncodedData)
        {
            try
            {
                byte[] base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                string a = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

                if (a.Contains("="))
                {
                    while (a.Contains("==") || (a.LastIndexOf("=") == (a.Length - 1)))
                    {
                        a = Utilities.UtilityAssistant.Base64Decode(a);
                    }
                }

                return a;//System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string Base64Decode(string): String: {0} | Message: {1}", base64EncodedData, ex.Message);
                return base64EncodedData;
            }
        }

        public static bool TryBase64Encode(string plainText, out string result)
        {
            try
            {
                byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                result = System.Convert.ToBase64String(plainTextBytes);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string TryBase64Encode(string): String: {0} | Message: {1}", plainText, ex.Message);
                result = plainText;
                return false;
            }
        }

        public static bool TryBase64Decode(string base64EncodedData, out string result)
        {
            try
            {
                byte[] base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                result = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

                if (result.Contains("="))
                {
                    while (result.Contains("==") || (result.LastIndexOf("=") == (result.Length - 1)))
                    {
                        result = Utilities.UtilityAssistant.Base64Decode(result);
                    }
                }

                return true;//System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error string TryBase64Decode(string): String: {0} | Message: {1}", base64EncodedData, ex.Message);
                Console.ResetColor();
                result = base64EncodedData;
                return false;
            }
        }
    }

    public static class StringExtensionMethods
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string ReplaceLast(this string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            return Source.Remove(place, Find.Length).Insert(place, Replace);
        }

        /*public static string ToJson(this Vector3 vector3)
        {
            try
            {
                return "";
            }
            catch(Exception ex) {
                return "";
            }
        }*/
    }
}