// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

namespace MvvmCross.Plugins.DownloadCache
{
    public interface IMvxLocalFileImageLoader<T>
    {
        Task<MvxImage<T>> Load(string localPath, bool shouldCache, int width, int height);
    }
}