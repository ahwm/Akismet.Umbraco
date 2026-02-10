export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Akismet Umbraco Dashboard",
    alias: "Akismet.Umbraco.Dashboard",
    type: "dashboard",
    js: () => import("./dashboard.element.js"),
    meta: {
      label: "Example Dashboard",
      pathname: "example-dashboard",
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Content",
      },
    ],
  },
];
