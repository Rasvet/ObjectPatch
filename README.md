# ObjectPatch

Simple utility to create diffs between two byte arrays and generates compact representation.

## Usage
```
* ObjectPatch diff chunk_size $original $modified $patch
```
Expects two files (```$original``` and ```$modified```) and outputs the ```$patch``` file.
```$chunk_size``` determines the size of each individual delta block.

```
* ObjectPatch patch $modified $patch $original
```
Expects two files (```$modified``` and ```$patch```) and outputs the ```$original``` file.
