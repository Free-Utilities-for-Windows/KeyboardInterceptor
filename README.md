# KeyboardInterceptor
Keyboard Interceptor is a keylogger application that logs keystrokes when either Notepad or Word is in focus.

## Installation

1. Run the bat script as administrator:
    ```
    install_keyboard_interceptor.bat
    ```
    Please note that this script adds the path of `KeyboardInterceptor.exe` to your system `PATH` environment variable. If your `PATH` variable is near or exceeding the maximum character limit (1024 characters), you may need to manually add the path or shorten your `PATH` variable.

2. Run the utility as administrator. For example:

    ```
    .\KeyboardInterceptor.exe
    ```

## Usage

After installation, you can run `KeyboardInterceptor.exe` from any command prompt window. The program will start logging the keys pressed when either Notepad or Word is in focus. The logs are saved in the `C:\KeyboardInterceptor` directory.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## Author

Bohdan Harabadzhyu
