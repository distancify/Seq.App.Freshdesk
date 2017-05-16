# Seq.App.Freshdesk
This is an app to create tickets in freshdesk from seq events. Its very basic in its implementation, but covers our current needs. If some feature is missing, feel free to open a pull request. :)

The app itself will not do any filtering, you should connect this to an already filtered signal.

## Features
- Api key or username/password login.
- Ticket severity based on log entry level.
- Ticket description built from event content.
- Optional subject prefix.
- Optional ticket type.

## Notes
Seq has more log levels than freshdesk has severities so i've mapped them like this.
- Debug, Verboe, Information -> Low
- Warning -> Medium
- Error -> High
- Fatal -> Urgent