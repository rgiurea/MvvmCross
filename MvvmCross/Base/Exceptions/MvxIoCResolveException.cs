﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;

namespace MvvmCross.Platform.Exceptions
{
    // Officially exception should support serialisation, but we don't add it here - mainly because of
    // serialization limits in PCLs
    public class MvxIoCResolveException : MvxException
    {
        public MvxIoCResolveException()
        {
        }

        public MvxIoCResolveException(string message)
            : base(message)
        {
        }

        public MvxIoCResolveException(string messageFormat, params object[] messageFormatArguments)
            : base(messageFormat, messageFormatArguments)
        {
        }

        // the order of parameters here is slightly different to that normally expected in an exception
        // - but this order allows us to put string.Format in place
        public MvxIoCResolveException(Exception innerException, string messageFormat, params object[] formatArguments)
            : base(innerException, messageFormat, formatArguments)
        {
        }
    }
}