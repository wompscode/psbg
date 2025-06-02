# Recursion in Templates
Prior to 1.1.0.0 (this is also the first version to actually have a version that is set - hooray me!):
- it was possible to insert a template that inserts the original inserting template leading to a stack overflow. Not great.  
  - psbg now quickly just checks where you came from, and if where you came from is referenced (`{{template: <template>}}`) in the current template, it will exit. It's not as graceful as if I just ignored it - but I'd rather not ignore something like this, you know?