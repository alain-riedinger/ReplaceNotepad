# ReplaceNotepad

A tool that permits to replace the Windows `Notepad.exe` with another editor
This is done using the Registry hack on registry key `Image File Execution Options`
(administrator rights required)
Syntax:
`ReplaceNotepad /install <Editor Path>`
    - installs <Editor Path> as replacement for Notepad
`ReplaceNotepad /uninstall`
    - uninstalls current replacement for Notepad
