export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "dashboard",
    alias: "Akismet.Umbraco.Dashboard",
    name: "Akismet Dashboard",
    element: () => import("./dashboard.element.js"),
    meta: {
      label: "Akismet",
      pathname: "akismet",
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Content",
      },
    ],
  },
];
