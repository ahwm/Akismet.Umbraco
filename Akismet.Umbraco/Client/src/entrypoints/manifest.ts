export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Akismet Umbraco Entrypoint",
    alias: "Akismet.Umbraco.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint.js"),
  },
];
