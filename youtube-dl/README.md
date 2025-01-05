# YouTube Downloader 

Example

```sh
dagger call with-urls --urls="[YOUTUBE_VIDEO_ID]" \
            with-options --options="-o,/app/video.mp4" \
            file --path=/app/video.mp4 \ 
            export --path ./video.mp4
```

```sh
dagger -m "https://github.com/pjmagee/dagerverse/youtube-dl" call \
            with-urls --urls="[YOUTUBE_VIDEO_ID]" \
            with-options --options="-o,/app/video.mp4" \
            file --path=/app/video.mp4 \ 
            export --path ./video.mp4
```
