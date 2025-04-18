﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.PowerFx.Types;
using NotImplementedException = System.NotImplementedException;

namespace Microsoft.PowerApps.TestEngine.Tests.PowerFx.Functions
{
    public class SomeOtherUntypedObject : IUntypedObject
    {
        public IUntypedObject this[int index] => throw new global::System.NotImplementedException();

        public bool TryGetPropertyNames(out IEnumerable<string> propertyNames)
        {
            throw new NotImplementedException();
        }

        public FormulaType Type
        {
            get
            {
                throw new global::System.NotImplementedException();
            }
        }

        public int GetArrayLength()
        {
            throw new global::System.NotImplementedException();
        }

        public bool GetBoolean()
        {
            throw new global::System.NotImplementedException();
        }

        public decimal GetDecimal()
        {
            throw new global::System.NotImplementedException();
        }

        public double GetDouble()
        {
            throw new global::System.NotImplementedException();
        }

        public string GetString()
        {
            throw new global::System.NotImplementedException();
        }

        public string GetUntypedNumber()
        {
            throw new global::System.NotImplementedException();
        }

        public bool TryGetProperty(string value, out IUntypedObject result)
        {
            throw new global::System.NotImplementedException();
        }
    }
}
