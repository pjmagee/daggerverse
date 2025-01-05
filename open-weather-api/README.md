# README

## Sample Usage

Default configuration

```bash
dagger -m "https://github.com/pjmagee/dagger-openweatherapi" --api-key=env:YOUR_API_KEY call location --name="London,UK"  
```

## Customise the configuration

```bash
dagger -m "https://github.com/pjmagee/dagger-openweatherapi" --api-key==env:YOUR_API_KEY --lang=fr
```