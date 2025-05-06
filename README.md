# psbg
phoebe's static blog generator
> heads up: psbg is **not complete**. changes can and **will** happen at any time.  
> keep this in mind when creating templates, config files and posts.  

# usage
> currently no builds are provided, so you'll need to build psbg yourself. i'll setup github actions for building cutting edge releases soon.
>
make posts in your post directory, `dotnet psbg.dll`, copy output in `output` (or whatever your output directory is set to) to whatever hosting solution you want.  

example `config.json` and templates included in `examples`. Put these files/folders in same directory as `psbg.dll`. proper documentation will be put together eventually, but for now just read these examples and piece it together.   

internal validation is now done, and if your template is broken in a way that would make the article unreadable, it will gracefully fail.  
there is also verbose validation that can be done by running `psbg validate_all` (list of commands available with `psbg help`).  
&nbsp;  

# building
get a .net9 build environment setup, run `dotnet restore`, then `dotnet build` :thumbsup: