const t = [
  {
    name: "Akismet Umbraco Entrypoint",
    alias: "Akismet.Umbraco.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-DpTJ6HQw.js")
  }
], a = [
  {
    type: "dashboard",
    alias: "Akismet.Umbraco.Dashboard",
    name: "Akismet Dashboard",
    element: () => import("./dashboard.element-Cwk_Yy0z.js"),
    meta: {
      label: "Akismet",
      pathname: "akismet"
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Settings"
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
