rem https://github.com/StefH/GitHubReleaseNotes

SET version=0.9.0

GitHubReleaseNotes --output ReleaseNotes.md --skip-empty-releases --exclude-labels question invalid doc --version %version% --token %GH_TOKEN%