﻿using NbtLib;
using Newtonsoft.Json;
using RedstoneSidekick.Domain.MinecraftSchematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RedstoneSidekick.Domain.MinecraftStructures
{
    public static class NBTConverter
    {
        public static Structure ConvertToStructure(string filePath)
        {
            var fileAsNBT = ReadFileAsNBT(filePath);

            var json = fileAsNBT.ToJsonString();

            var jsonWithoutInvalidNBT = json.RemoveInvalidNBT();

            return JsonConvert.DeserializeObject<Structure>(jsonWithoutInvalidNBT);
        }

        public static Schematic ConvertToSchematic(string filePath)
        {
            var fileAsNBT = ReadFileAsNBT(filePath);

            var json = fileAsNBT.ToJsonString();

            var jsonWithoutInvalidNBT = json.RemoveInvalidNBT().RemoveSchematicPaletteStates().SwapSchematicPaletteDictionary();

            return JsonConvert.DeserializeObject<Schematic>(jsonWithoutInvalidNBT);
        }

        private static NbtCompoundTag ReadFileAsNBT(string filePath)
        {
            using (var inputStream = File.OpenRead(filePath))
            {
                return NbtConvert.ParseNbtStream(inputStream);
            }
        }

        private static string RemoveInvalidNBT(this string json)
        {
            int safeguard = 0;
            while (json.Contains("nbt") && safeguard < 10000)
            {
                var startIndex = json.IndexOf("nbt") - 1;
                var endIndex = json.IndexOf("pos", startIndex) - 1;
                json = json.Remove(startIndex, endIndex - startIndex);
                safeguard++;
            }

            return json;
        }

        private static string SwapSchematicPaletteDictionary(this string json)
        {
            var regex = @"(\w+:\w+)[\"":\s*]+(\d +)";

            json = Regex.Replace(json, regex, "$2\": \"$1\"", RegexOptions.IgnorePatternWhitespace);

            return json;
        }

        private static string RemoveSchematicPaletteStates(this string json)
        {
            var regex = @"(?<=\w)\[.*?\](?=\"")";

            json = Regex.Replace(json, regex, "");

            return json;
        }
    }
}
