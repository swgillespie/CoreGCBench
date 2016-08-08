## CoreCLR GC Performance Benchmarking

`CoreGCBench` is a collection of three utilities for benchmarking the CoreCLR garbage collector.
Its components are:

* `CoreGCBench.Runner`, a runner that executes benchmarks and gathers traces
   for each benchmark, 
* `CoreGCBench.Analysis`, a library that takes the output of the runner
   and analyzes it, calculating a number of GC-related metrics, and (if multiple benchmark runs
   are provided) analyzes the data for potential regressions or improvements,
* `CoreGCBench.Analysis.Runner`, a command-line driver for `CoreGCBench.Analysis` that
   performs standalone and side-by-side analyses of benchmark runs.

`CoreGCBench` is a new utility and may be unstable or have pain points. I will be working
to reduce these pain points as development continues, so feel free to drop me a line or file
issues on things that annoy you and I will do my best to attend to them.

If you'd like to use these utilities, check out the documentation: https://github.com/swgillespie/CoreGCBench/wiki.
All documentation will be centered around the wiki.

### Known Issues
* [Control+C leaks a PerfView session and is a huge pain to clean up](https://github.com/swgillespie/CoreGCBench/issues/2) -
Until this is fixed you should not Control+C the runner process.