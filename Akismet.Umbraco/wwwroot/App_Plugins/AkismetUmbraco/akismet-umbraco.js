const a = [
  {
    name: "Akismet Umbraco Entrypoint",
    alias: "Akismet.Umbraco.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-DpTJ6HQw.js")
  }
], t = [
  {
    name: "Akismet Umbraco Dashboard",
    alias: "Akismet.Umbraco.Dashboard",
    type: "dashboard",
    js: () => import("./dashboard.element-CLkzx9mZ.js"),
    meta: {
      label: "Example Dashboard",
      pathname: "example-dashboard"
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Content"
      }
    ]
  }
], o = [
  ...a,
  ...t
];
export {
  o as manifests
};
//# sourceMappingURL=akismet-umbraco.js.map
