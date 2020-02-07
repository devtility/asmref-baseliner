// Copyright (c) 2020 Devtility.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the repo root for license information.

using Microsoft.Build.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DumpAsmRefs.Tests.Infrastructure
{
    internal class FakeBuildEngine : IBuildEngine
    {
        public IList<CustomBuildEventArgs> CustomEvents { get; } = new List<CustomBuildEventArgs>();
        public IList<BuildErrorEventArgs> ErrorEvents { get; } = new List<BuildErrorEventArgs>();
        public IList<BuildMessageEventArgs> MessageEvents { get; } = new List<BuildMessageEventArgs>();
        public IList<BuildWarningEventArgs> WarningsEvents { get; } = new List<BuildWarningEventArgs>();

        #region IBuildEngine implementation

        bool IBuildEngine.ContinueOnError => false;

        int IBuildEngine.LineNumberOfTaskNode => 21;

        int IBuildEngine.ColumnNumberOfTaskNode => 42;

        string IBuildEngine.ProjectFileOfTaskNode => "{project file of task node}";

        bool IBuildEngine.BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        void IBuildEngine.LogCustomEvent(CustomBuildEventArgs e)
        {
            CustomEvents.Add(e);
        }

        void IBuildEngine.LogErrorEvent(BuildErrorEventArgs e)
        {
            ErrorEvents.Add(e);
        }

        void IBuildEngine.LogMessageEvent(BuildMessageEventArgs e)
        {
            MessageEvents.Add(e);
        }

        void IBuildEngine.LogWarningEvent(BuildWarningEventArgs e)
        {
            WarningsEvents.Add(e);
        }

        #endregion IBuildEngine implementation
    }
}
