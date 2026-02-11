import { LitElement as w, html as u, css as $, state as p, customElement as z } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as D } from "@umbraco-cms/backoffice/element-api";
import { UMB_NOTIFICATION_CONTEXT as E } from "@umbraco-cms/backoffice/notification";
import { c as i } from "./client.gen-DHCnB5Mt.js";
class l {
  static ping(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/ping",
      ...t
    });
  }
  static whatsMyName(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/whatsMyName",
      ...t
    });
  }
  static whatsTheTimeMrWolf(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/whatsTheTimeMrWolf",
      ...t
    });
  }
  static whoAmI(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/whoAmI",
      ...t
    });
  }
  // Akismet endpoints
  static verifyKey(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/verify-key",
      ...t
    });
  }
  static getStats(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/stats",
      ...t
    });
  }
  static getComments(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/comments",
      ...t
    });
  }
  static getSpamComments(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/spam",
      ...t
    });
  }
  static getComment(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: `/umbraco/akismetumbraco/api/v1/comment/${t.path.id}`,
      ...t
    });
  }
  static getSpamCount(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/spam-count",
      ...t
    });
  }
  static getHamCount(t) {
    return (t?.client ?? i).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/ham-count",
      ...t
    });
  }
  static deleteComment(t) {
    return (t?.client ?? i).delete({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: `/umbraco/akismetumbraco/api/v1/comment/${t.path.id}`,
      ...t
    });
  }
  static reportHam(t) {
    return (t?.client ?? i).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: `/umbraco/akismetumbraco/api/v1/report-ham/${t.path.id}`,
      ...t
    });
  }
  static reportSpam(t) {
    return (t?.client ?? i).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: `/umbraco/akismetumbraco/api/v1/report-spam/${t.path.id}`,
      ...t
    });
  }
  static checkComment(t) {
    return (t?.client ?? i).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/akismetumbraco/api/v1/check",
      ...t
    });
  }
}
var A = Object.defineProperty, M = Object.getOwnPropertyDescriptor, k = (e) => {
  throw TypeError(e);
}, n = (e, t, a, m) => {
  for (var r = m > 1 ? void 0 : m ? M(t, a) : t, v = e.length - 1, d; v >= 0; v--)
    (d = e[v]) && (r = (m ? d(t, a, r) : d(r)) || r);
  return m && r && A(t, a, r), r;
}, f = (e, t, a) => t.has(e) || k("Cannot " + a), o = (e, t, a) => (f(e, t, "read from private field"), a ? a.call(e) : t.get(e)), h = (e, t, a) => t.has(e) ? k("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, a), T = (e, t, a, m) => (f(e, t, "write to private field"), t.set(e, a), a), N = (e, t, a) => (f(e, t, "access private method"), a), s, b, C, y, g;
let c = class extends D(w) {
  constructor() {
    super(), h(this, b), this._keyValid = !1, this._spamComments = [], this._loading = !1, this._spamCount = 0, this._hamCount = 0, h(this, s), h(this, y, async (e) => {
      const { error: t } = await l.deleteComment({ path: { id: e.toString() } });
      if (t) {
        o(this, s) && o(this, s).peek("danger", {
          data: {
            headline: "Error",
            message: "Failed to delete comment"
          }
        });
        return;
      }
      o(this, s) && o(this, s).peek("positive", {
        data: {
          headline: "Success",
          message: "Comment deleted successfully"
        }
      }), this.loadData();
    }), h(this, g, async (e) => {
      const { error: t } = await l.reportHam({ path: { id: e.toString() } });
      if (t) {
        o(this, s) && o(this, s).peek("danger", {
          data: {
            headline: "Error",
            message: "Failed to report ham"
          }
        });
        return;
      }
      o(this, s) && o(this, s).peek("positive", {
        data: {
          headline: "Success",
          message: "False positive reported to Akismet"
        }
      }), this.loadData();
    }), this.consumeContext(E, (e) => {
      T(this, s, e);
    }), this.loadData();
  }
  async loadData() {
    this._loading = !0;
    const { data: e, error: t } = await l.verifyKey();
    !t && e && (this._keyValid = e);
    const { data: a, error: m } = await l.getStats();
    !m && a && (this._stats = a);
    const { data: r, error: v } = await l.getSpamCount();
    !v && r !== void 0 && (this._spamCount = r);
    const { data: d, error: x } = await l.getHamCount();
    !x && d !== void 0 && (this._hamCount = d);
    const { data: _, error: S } = await l.getSpamComments();
    !S && _ && (this._spamComments = _), this._loading = !1;
  }
  render() {
    return this._loading ? u`
        <uui-loader-bar></uui-loader-bar>
      ` : u`
      <uui-box headline="Akismet Status">
        ${this._keyValid ? u`
              <div class="status-box success">
                <uui-icon name="icon-check"></uui-icon>
                <span>API Key Valid</span>
              </div>
            ` : u`
              <div class="status-box error">
                <uui-icon name="icon-alert"></uui-icon>
                <span>API Key not found or not valid - please check Configuration</span>
              </div>
            `}

            ${this._stats ? u`
            <uui-box headline="Statistics">
              <div class="stats-grid">
                <div class="stat-item">
                  <div class="stat-value">${this._stats.spam?.toLocaleString() ?? 0}</div>
                  <div class="stat-label">Spam Blocked</div>
                </div>
                <div class="stat-item">
                  <div class="stat-value">${this._stats.ham?.toLocaleString() ?? 0}</div>
                  <div class="stat-label">Ham (Not Spam)</div>
                </div>
                <div class="stat-item">
                  <div class="stat-value">${this._stats.accuracy?.toFixed(1) ?? 0}%</div>
                  <div class="stat-label">Accuracy</div>
                </div>
                <div class="stat-item">
                  <div class="stat-value">${N(this, b, C).call(this, this._stats.timeSaved ?? 0)}</div>
                  <div class="stat-label">Time Saved</div>
                </div>
              </div>
            </uui-box>
          ` : ""}

      <uui-box headline="Database Statistics">
        <div class="stats-grid">
          <div class="stat-item">
            <div class="stat-value">${this._spamCount.toLocaleString()}</div>
            <div class="stat-label">Spam in Database</div>
          </div>
          <div class="stat-item">
            <div class="stat-value">${this._hamCount.toLocaleString()}</div>
            <div class="stat-label">Ham in Database</div>
          </div>
        </div>
      </uui-box>

      <uui-box headline="Recent Spam Comments">
        ${this._spamComments.length === 0 ? u`<p>No spam comments found.</p>` : u`
              <div class="comments-list">
                ${this._spamComments.slice(0, 10).map(
      (e) => u`
                    <div class="comment-item">
                      <div class="comment-header">
                        <strong>${e.userName ?? "Anonymous"}</strong>
                        <span class="comment-date">${new Date(e.commentDate).toLocaleString()}</span>
                      </div>
                      <div class="comment-text">${e.commentText}</div>
                      <div class="comment-actions">
                        <uui-button
                          look="secondary"
                          label="Report Ham"
                          @click="${() => o(this, g).call(this, e.id)}"
                        >
                          Report as Not Spam
                        </uui-button>
                        <uui-button
                          look="primary"
                          color="danger"
                          label="Delete"
                          @click="${() => o(this, y).call(this, e.id)}"
                        >
                          Delete
                        </uui-button>
                      </div>
                    </div>
                  `
    )}
              </div>
            `}
      </uui-box>
    `;
  }
};
s = /* @__PURE__ */ new WeakMap();
b = /* @__PURE__ */ new WeakSet();
C = function(e) {
  return e < 3600 ? `${Math.round(e / 60)} minutes` : e < 86400 ? `${Math.round(e / 3600)} hours` : `${Math.round(e / 86400)} days`;
};
y = /* @__PURE__ */ new WeakMap();
g = /* @__PURE__ */ new WeakMap();
c.styles = [
  $`
      :host {
        display: block;
        padding: var(--uui-size-layout-1);
      }

      uui-box {
        margin-bottom: var(--uui-size-layout-1);
      }

      .status-box {
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-3);
        padding: var(--uui-size-space-4);
        border-radius: var(--uui-border-radius);
      }

      .status-box.success {
        background-color: var(--uui-color-positive-emphasis);
        color: var(--uui-color-positive-contrast);
      }

      .status-box.error {
        background-color: var(--uui-color-danger-emphasis);
        color: var(--uui-color-danger-contrast);
      }

      .stats-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: var(--uui-size-layout-1);
      }

      .stat-item {
        padding: var(--uui-size-space-5);
        border: 1px solid var(--uui-color-border);
        border-radius: var(--uui-border-radius);
        text-align: center;
      }

      .stat-value {
        font-size: var(--uui-type-h2-size);
        font-weight: bold;
        color: var(--uui-color-interactive);
        margin-bottom: var(--uui-size-space-2);
      }

      .stat-label {
        font-size: var(--uui-type-small-size);
        color: var(--uui-color-text-alt);
      }

      .comments-list {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-4);
      }

      .comment-item {
        padding: var(--uui-size-space-4);
        border: 1px solid var(--uui-color-border);
        border-radius: var(--uui-border-radius);
      }

      .comment-header {
        display: flex;
        justify-content: space-between;
        margin-bottom: var(--uui-size-space-2);
      }

      .comment-date {
        color: var(--uui-color-text-alt);
        font-size: var(--uui-type-small-size);
      }

      .comment-text {
        margin-bottom: var(--uui-size-space-3);
        color: var(--uui-color-text);
      }

      .comment-actions {
        display: flex;
        gap: var(--uui-size-space-2);
      }
    `
];
n([
  p()
], c.prototype, "_keyValid", 2);
n([
  p()
], c.prototype, "_stats", 2);
n([
  p()
], c.prototype, "_spamComments", 2);
n([
  p()
], c.prototype, "_loading", 2);
n([
  p()
], c.prototype, "_spamCount", 2);
n([
  p()
], c.prototype, "_hamCount", 2);
c = n([
  z("akismet-dashboard")
], c);
const L = c;
export {
  c as AkismetDashboardElement,
  L as default
};
//# sourceMappingURL=dashboard.element-Cwk_Yy0z.js.map
