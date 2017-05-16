# Seq.App.Freshdesk
This is an app to create tickets in freshdesk from seq events. Its very basic in its implementation, but covers our current needs. If some feature is missing, feel free to open a pull request. :)

The app itself will not do any filtering, you should connect this to an already filtered signal.

## Features
- Api key or username/password login.
- Ticket level based on log entry level.
- Ticket description built from event content.
- Optional subject prefix.
- Optional ticket type.