using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VolumeDataReader
{
    private static string dataPath = Application.dataPath + "/Resources/";

        public static float[,,] ReadBrainData()
    {
        float[,,] output = new float[256, 109, 256];
        float max = 0;
        float min = float.MaxValue;
        int dataLength = 0;

        for (int i = 1; i <= 109; i++)
        {
            string filePath = dataPath + "BrainScans/MRBrain." + i;
            byte[] data = File.ReadAllBytes(filePath);
            dataLength = data.Length;

            // Now you can access the data byte by byte
            for (int j = 0; j < dataLength; j += 2)
            {
                float valueAtPoint = (int) ((data[j] << 8) | (data[j + 1] & 0xFF));
                if (valueAtPoint > max) {
                    max = valueAtPoint;
                }
                min = Mathf.Min(valueAtPoint, min);
                output[j / 2 % 256, i - 1, j / 512] = valueAtPoint;
            }
        }

        for (int i = 1; i <= 109; i++)
        {
            for (int j = 0; j < dataLength; j += 2)
            {
                float valueAtPoint = output[j / 2 % 256, i - 1, j / 512];
                output[j / 2 % 256, i - 1, j / 512] = (valueAtPoint - min) / (max - min);
            }
        }

        return output;

    }

    public static float[,,] ReadSkullData()
    {
        float[,,] output = new float[256, 113, 256];
        float max = 0;
        float min = float.MaxValue;
        int dataLength = 0;

        for (int i = 1; i <= 113; i++)
        {
            string filePath = dataPath + "SkullScans/CThead." + i;
            byte[] data = File.ReadAllBytes(filePath);
            dataLength = data.Length;

            // Now you can access the data byte by byte
            for (int j = 0; j < dataLength; j += 2)
            {
                float valueAtPoint = (int) ((data[j] << 8) | (data[j + 1] & 0xFF));
                if (valueAtPoint > max) {
                    max = valueAtPoint;
                }
                min = Mathf.Min(valueAtPoint, min);
                output[j / 2 % 256, i - 1, j / 512] = valueAtPoint;
            }
        }

        for (int i = 1; i <= 113; i++)
        {
            for (int j = 0; j < dataLength; j += 2)
            {
                float valueAtPoint = output[j / 2 % 256, i - 1, j / 512];
                output[j / 2 % 256, i - 1, j / 512] = (valueAtPoint - min) / (max - min);
            }
        }

        return output;

    }
}
