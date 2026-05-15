const t = [
  {
    name: "Akismet Umbraco Entrypoint",
    alias: "Akismet.Umbraco.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-B_MFrulr.js")
  }
], a = [
  {
    type: "dashboard",
    alias: "Akismet.Umbraco.Dashboard",
    name: "Akismet Dashboard",
    element: () => import("./dashboard.element-DDeSIVtY.js"),
    meta: {
      label: "Akismet",
      pathname: "akismet"
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Content"
      }
    ]
  }
], i = [
  ...t,
  ...a
];
export {
  i as manifests
};
//# sourceMappingURL=akismet-umbraco.js.map
