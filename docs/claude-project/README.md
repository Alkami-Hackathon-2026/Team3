# Setting up the "Alkami SDK" Claude Project

What is in this bundle

- `CLAUDE.md`: drop-in instructions file for Claude Code at the root of an Alkami SDK repo (or `~/.claude/CLAUDE.md` for all repos). Points to `docs/alkami-sdk/`.
- `docs/alkami-sdk/00-index.md` plus 15 topic docs: the distilled reference. Same files serve Claude Code (in the repo) and the Claude Project (uploaded as knowledge).
- `docs/alkami-sdk/source/`: the 386 raw Confluence page exports (id, title, parent, updated date, URL, body). Ground truth for grepping; not needed in the Claude Project.
- `claude-project/PROJECT_INSTRUCTIONS.md`: text for the Project's Instructions box.

Steps

1. In Claude, create a Project named "Alkami SDK".
2. Open the Project's instructions and paste the contents of `claude-project/PROJECT_INSTRUCTIONS.md`. Edit the first line if you want it scoped to your FI, and add your well-known identifier (Jira project prefix) so Claude uses it in naming examples.
3. Upload the 15 topic docs from `docs/alkami-sdk/` (not the `source/` folder) as project knowledge. They total about 690K characters. If the project's knowledge capacity complains, drop `12-releases-changelog-and-events.md`, `10-alkami-embedded-and-native-apps.md`, and `09c-albus-build-tool.md` first; they are the least used day to day.
4. Optionally upload `CLAUDE.md` too; it is a compact rules summary Claude can cite quickly.
5. Test with prompts like "Scaffold a provider-based microservice named for our FI that stores a vendor API key as a setting" or "Review this nuspec against the submission checklist".

For Claude Code

1. Copy `CLAUDE.md` to the repo root and `docs/alkami-sdk/` next to it (keep `source/` if you want grep-able originals; add it to `.gitignore` if the repo is shared outside the FI).
2. Replace `<FI>` in `CLAUDE.md` with your well-known identifier and set the SDK version line.
3. If you keep a per-project CLAUDE.md as well, keep this one at the repo root and put project-specific notes (service name, settings, Jira ticket) in the project folder's CLAUDE.md.

Refreshing

The Confluence space changes (Standard-SSO tree was rewritten in 2026, releases page updated 2026-08). To refresh, re-export the space pages via the Confluence REST API (`/rest/api/content?spaceKey=SDKC&type=page&limit=200&expand=version` for the list, `/rest/api/content/<id>?expand=body.view` for each body), diff against `docs/alkami-sdk/source/`, and update the affected topic doc. The `Recent Updates` panel on the space home page is the quickest way to spot changes.
