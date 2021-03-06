# WaifuLabsClient

When you first launch the program use the command `dir`, this will take you to the folder generated waifus appear. Now simply type `"gen"`, this will execute the `gen` command with default parameters "gen (default seed) s3", your folder should look like [this](https://user-images.githubusercontent.com/66906618/147149232-9b1d9da0-0594-4721-ae49-595ca9a3c75f.png).

Parameters using "()" are optional while parameters using "[]" are required.

#### Use `gen (waifu selector) (s[stage]) (count)` to generate waifus.

* s[stage number] - s0 = generate a random waifu, s1 = generate the same waifu but in different colours, s2 = generate similar waifus, s3 = generate the same waifu but in different poses.

### Waifu Selection Parameters
These parameters work for `gen` and `save`
* w[waifu number] - use the waifu specified by [waifu number] as a seed for the generation
* l - use the seed you used in the last `gen` command
* [seed] - you can also use a raw seed. Example: [706925,706925,706925,706925,808825,808825,808825,808825,808825,808825,808825,808825,808825,808825,808825,808825,0,[109.855,96.4925,94.6325]]

[Examples Commands](https://github.com/hopto-dot/WaifuLabsClient/wiki/Example-Commands)

### Defaults parameters
* If you don't use the `s` parameter, a default of `s3` will be used.
* if you don't use the `w` or `l`, the seed inside of seed.txt will be used.

### save
Syntax: `save [waifu selector]` - save the seed of a waifu to saved_waifus.txt

log.txt holds all the seeds of the last generation.

## Known issue: "Failed to convert image to byte array."
If you see this error, you can trying running the command 1 to 5 more times until it works, if not, give up. This will be fixed in the future.
