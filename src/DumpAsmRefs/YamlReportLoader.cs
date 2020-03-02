// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using DumpAsmRefs.Interfaces;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DumpAsmRefs
{
    public class YamlReportLoader : IReportLoader
    {
        #region IReportLoader methods

        public AsmRefResult Load(string data)
        {
            var input = new StringReader(data);
            var deserializer = new DeserializerBuilder().Build();

            var parser = new Parser(input);
            parser.Consume<StreamStart>();

            InputCriteria inputCriteria = null;
            var asmRefSections = new List<AssemblyReferenceInfo>();

            // Read header document
            if (parser.Accept<DocumentStart>(out _))
            {
                inputCriteria = deserializer.Deserialize<InputCriteria>(parser);
            }

            // Any other docs should contain info about a single assembly and its references
            while (parser.Accept<DocumentStart>(out _))
            {
                var asmRefSection = deserializer.Deserialize<AssemblyReferenceInfo>(parser);
                if (asmRefSection != null)
                {
                    asmRefSections.Add(asmRefSection);
                }
            }

            var result = new AsmRefResult(inputCriteria ?? new InputCriteria(), asmRefSections);
            return result;
        }

        #endregion

        public static T Deserialize<T>(string data)
        {
            var deser = new Deserializer();
            T obj = deser.Deserialize<T>(data);
            return obj;
        }
    }
}
