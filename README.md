# psbg
phoebe's static blog generator
> heads up: psbg is **not complete, very janky, and very much designed around my own wants and needs.** changes can and **will** happen at any time.  
> 
> keep this in mind when creating templates, config files and posts. you should 100% consider using any other stable static site generator (it's even likely that eventually, once i get sick of working on this that even i'll move to something else.)  
> 
> notices of changes are in [notices/](notices/).
# usage
> currently no builds are provided, so you'll need to build psbg yourself. i'll setup github actions for building cutting edge releases soon.
>
make posts in your post directory, `dotnet psbg.dll`, copy output in `output` (or whatever your output directory is set to) to whatever hosting solution you want.  
example `config.json` and templates included in `examples`. Put these files/folders in same directory as `psbg.dll`. proper documentation will be put together eventually, but for now just read these examples and piece it together.   

internal validation is now done, and if your template is broken in a way that would make the article unreadable, it will gracefully fail.  
there is also verbose validation that can be done by running `psbg validate_all` (list of commands available with `psbg help`). you can also skip validation by setting SkipValidation to true in your config. you should do this if your page structure is not similar to the example ones. (it is intended to match how my pages are structured.)  

validation needs to be redone to match how parsing functions now: it does not validate recursively. i will fix this eventually, but for now you should just skip validation if it becomes an issue for you. [validation is automatically skipped currently: upon fix it will be reenabled unless the config flag `SkipValidation` is true.]
&nbsp;  
# building
get a .net9 build environment setup, run `dotnet restore`, then `dotnet build` :thumbsup: