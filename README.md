<h1 align="center">Texture Channel Packer</h1>

<div align="center">
  
  Unity editor tool for taking RGBA color channels from source textures and merging them into one texture.

  &nbsp;<img src="https://img.shields.io/badge/Unity-2022.3+-lightgray" />
</div>

&nbsp;
![Preview](https://github.com/user-attachments/assets/d55dc3d8-ea8d-4c87-b6f3-2b8aa600aaea)

## Install
You can get the package through Unity's package manager by copying the git url and clicking "Install package from git URL...".

```
https://github.com/Lasriel/TextureChannelPacker.git
```

![PackageManagerInstall](https://github.com/user-attachments/assets/e0bfd968-3a3f-42e7-9473-04d81aeab3c8)

or

Adding the package manually to `Packages/manifest.json` in your Unity project by copying the following to dependencies:
```
"moe.lasriel.texture-channel-packer": "https://github.com/Lasriel/TextureChannelPacker.git"
```

## Usage
The tool can be opened from: `Window/Lasriel/Texture Channel Packer`

### Input texture
Each input is marked with a colored dot, followed by the input texture.
You can select which color channel to copy from the input texture with the dropdown next to the texture field.
Input color channels can be inverted by using the invert toggles.

![UsageInput](https://github.com/user-attachments/assets/53d962bb-2820-4d6f-aec9-051abf044b38)

### Output settings
There are two settings for the output texture:

**Format:** Image format to encode the texture to.

**Resolution:** Texture width and height.

![UsageSettings](https://github.com/user-attachments/assets/e0baabc5-c53c-460f-b140-1e23b5f7c0e0)

### Preview
Shows a preview of the packed texture and allows you to see all the color channels of the output.
You can also toggle the transparency of the preview.

![PreviewTex0](https://github.com/user-attachments/assets/393c931a-3f01-4854-bf7a-4a170ec99bde)
![PreviewTex1](https://github.com/user-attachments/assets/631f5652-b525-4ab8-8730-db6ed4afadc0)
![PreviewTex2](https://github.com/user-attachments/assets/bc281d15-f27c-4cfc-8ddc-4374f4ace993)

## License

This project is published under [MIT License](/LICENSE).

Unity-chan image used in previews is licensed under [Unity-Chan License Terms.](https://unity-chan.com/contents/license_en/)
