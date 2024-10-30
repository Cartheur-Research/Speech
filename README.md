## Cartheur Speech

Making speech possible regardless of platform.

### Useful Links

* [espeak](http://espeak.sourceforge.net/)
* [docs](https://espeak.sourceforge.net/docindex.html)

### Use cases

The eSpeak binary generates sounds, pitches, speed, gaps, languages, and even accents.

To modify variables in the program, use the following code blocks.

```
        Speaker speaker;

        private void SpeedChanged(object sender, EventArgs e)
        {
            speaker.Speed = Convert.ToInt32(nmSpeed.Value);
        }

        private void PitchChanged(object sender, EventArgs e)
        {
            speaker.Pitch = Convert.ToInt32(nmPitch.Value);
        }

        private void LanguageChanged(object sender, EventArgs e)
        {
            speaker.VoiceLanguage = txtLanguage.Text;
        }

        private void VariantChanged(object sender, EventArgs e)
        {
            speaker.Variant = (Variant)cbVariants.SelectedItem;
        }
```
To call the program to speak in different ways. Speak with a path to a file, and speaking text directly.

```
        private async void SpeakTextFile(object sender, EventArgs e)
        {
            await speaker.SpeakTextFileAsync(<TextFilePath>);
        }

        private async void SpeakText(object sender, EventArgs e)
        {
            await speaker.SpeakTextAsync(<Text>);
        }
```

