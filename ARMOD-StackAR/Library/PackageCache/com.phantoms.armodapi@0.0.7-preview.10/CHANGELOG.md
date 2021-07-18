# 0.0.7-preview.10
## Change
- Obsolete `API.InstanceGameObject` api. Use `Object.Instantiate` instead

# 0.0.7-preview.9
## Change
- To instantiate the API, you need to pass in the project name
- Remove all parameters that need to be passed in the project name method
## Add
- Load `ComputeShader` Supported

# 0.0.7-preview.8
## Change

-  Change`TryAcquireCurrentFrame` API to `TryAcquireCurrentFrame(TryAcquireCurrentFrameNotificationData _data)`

# 0.0.7-preview.7
## Add
- New API of `ResizeARWorldScale`

# 0.0.7-preview.6
## Fix
- Fix the performance of `CheckFeatureAvailability`
- Fix the performance of `TryAcquireAROcclusionFrame` and add paramater `AROcclusionNotificationData`

# 0.0.7-preview.5

## New
- Add new method `TryAcquireAROcclusionFrame()` for developer get AROcclusion images 
- Add `LoadAssetsAsync()`
- Add `LoadAsset<T>()`
- Add `LoadMaterial()`

# 0.0.7-preview.4
## Fix
- Remove `LoadAssetAsync` Obsolete
  
## New
- Add CHANGELOG.md file
