# Keysight Medalist i3070 Converter

Converts Keysight Medalist i3070 test result files in the i3070 Log Record Format to WATS UUT reports.

## Integration Details

| Property | Value |
|----------|-------|
| **Category** | WATS Client converter |
| **Type** | FileConverter |
| **Format** | TXT |
| **Test type** | ICT |

## About

The Keysight Medalist i3070 (originally HP 3070, later Agilent 3070) is one of the most widely used in-circuit test systems. The i3070 Systems Software controller produces a structured **Log Record Format** where each record is a `{@TAG|...}` token. This converter handles the standard Log Record Format covering analog limit tests (LIM2/LIM3) and pass/fail steps.

For broader test type coverage (digital, boundary scan, topology scan), use the [extended variant](../MedalistI3070Extended/). For older pipe-delimited files, use the [legacy flat-file converter](../Agilent3070/).

## Getting Started

* [What is WATS?](https://wats.com)
* [WATS Client download](https://wats.com/download)
* [Setting up a custom converter](https://support.wats.com/hc/en-us/articles/13344321749788-Setting-up-a-custom-converter)

## Download

The recommended installation method is via the MSI installer. Download the latest release from the [Releases](https://github.com/TheWATSCompany/WATS-Converter-Keysight-ICT3070/releases/latest) page.

## Installation

### Using the MSI Installer (Recommended)

1. Download the `.msi` file from the [Releases](https://github.com/TheWATSCompany/WATS-Converter-Keysight-ICT3070/releases/latest) page.
2. Run the installer - it will automatically place the converter in the correct WATS Client folder.
3. Restart the WATS Client Service.

### Manual DLL Installation

1. Download the `.dll` file from the [Releases](https://github.com/TheWATSCompany/WATS-Converter-Keysight-ICT3070/releases/latest) page.
2. In the WATS Client Configurator, go to Converters, click Add, and browse for the downloaded DLL.
3. Select the appropriate converter class from the drop-down.

## Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `operationTypeCode` | 10 | Operation type code when missing from the log file. |

## Contributing

We welcome contributions! Feel free to open an issue or create a pull request.

## Troubleshooting

### Converter failed to start

* Ensure the WATS Client Service has folder permission to the input path.
* Restart the WATS Client Service after configuration changes.

### Converter class drop-down is empty

* The DLL file may be blocked by Windows. Right-click the file, open Properties, and click Unblock.

### Other issues

Contact [WATS Support](mailto:support@wats.com) and include the `wats.log` file.

## Resources

* [GitHub Repository](https://github.com/TheWATSCompany/WATS-Converter-Keysight-ICT3070)
* [Keysight 3070 product page](https://www.keysight.com)
* [WATS Documentation](https://support.wats.com)
* [Setting up a custom converter](https://support.wats.com/hc/en-us/articles/13344321749788-Setting-up-a-custom-converter)

## License

See [LICENSE](LICENSE.md) for details.
