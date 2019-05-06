using System;

namespace Redux
{
    public delegate IAction Dispatcher(IAction action , IProgress<int> progress);
}