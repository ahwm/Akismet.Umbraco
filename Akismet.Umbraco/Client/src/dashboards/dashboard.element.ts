import {
  LitElement,
  css,
  html,
  customElement,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { AkismetUmbracoService } from "../api/index.js";
import type { AkismetSubmission, SpamStats } from "../api/types.gen.js";

@customElement("akismet-dashboard")
export class AkismetDashboardElement extends UmbElementMixin(LitElement) {
  @state()
  private _keyValid: boolean = false;

  @state()
  private _stats?: SpamStats;

  @state()
  private _spamComments: AkismetSubmission[] = [];

  @state()
  private _loading: boolean = false;

  @state()
  private _spamCount: number = 0;

  @state()
  private _hamCount: number = 0;

  #notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

  constructor() {
    super();

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
      this.#notificationContext = notificationContext;
    });

    this.loadData();
  }

  async loadData() {
    this._loading = true;

    // Verify key
    const { data: keyValid, error: keyError } = await AkismetUmbracoService.verifyKey();
    if (!keyError && keyValid) {
      this._keyValid = keyValid;
    }

    // Get stats
    const { data: stats, error: statsError } = await AkismetUmbracoService.getStats();
    if (!statsError && stats) {
      this._stats = stats as SpamStats;
    }

    // Get spam count
    const { data: spamCount, error: spamCountError } = await AkismetUmbracoService.getSpamCount();
    if (!spamCountError && spamCount !== undefined) {
      this._spamCount = spamCount;
    }

    // Get ham count
    const { data: hamCount, error: hamCountError } = await AkismetUmbracoService.getHamCount();
    if (!hamCountError && hamCount !== undefined) {
      this._hamCount = hamCount;
    }

    // Get spam comments
    const { data: spamComments, error: spamError } = await AkismetUmbracoService.getSpamComments();
    if (!spamError && spamComments) {
      this._spamComments = spamComments as AkismetSubmission[];
    }

    this._loading = false;
  }

  #formatTimeSaved(seconds: number): string {
    if (seconds < 3600) {
      return `${Math.round(seconds / 60)} minutes`;
    } else if (seconds < 86400) {
      return `${Math.round(seconds / 3600)} hours`;
    } else {
      return `${Math.round(seconds / 86400)} days`;
    }
  }

  #onDeleteComment = async (id: number) => {
    const { error } = await AkismetUmbracoService.deleteComments({ path: { ids: id.toString() } });
    
    if (error) {
      if (this.#notificationContext) {
        this.#notificationContext.peek("danger", {
          data: {
            headline: "Error",
            message: "Failed to delete comment",
          },
        });
      }
      return;
    }

    if (this.#notificationContext) {
      this.#notificationContext.peek("positive", {
        data: {
          headline: "Success",
          message: "Comment deleted successfully",
        },
      });
    }

    // Reload data
    this.loadData();
  };

  #onReportHam = async (id: number) => {
    const { error } = await AkismetUmbracoService.reportHam({ path: { id: id.toString() } });
    
    if (error) {
      if (this.#notificationContext) {
        this.#notificationContext.peek("danger", {
          data: {
            headline: "Error",
            message: "Failed to report ham",
          },
        });
      }
      return;
    }

    if (this.#notificationContext) {
      this.#notificationContext.peek("positive", {
        data: {
          headline: "Success",
          message: "False positive reported to Akismet",
        },
      });
    }

    // Reload data
    this.loadData();
  };

  render() {
    if (this._loading) {
      return html`
        <uui-loader-bar></uui-loader-bar>
      `;
    }

    return html`
      <uui-box headline="Akismet Status">
        ${this._keyValid
          ? html`
              <div class="status-box success">
                <uui-icon name="icon-check"></uui-icon>
                <span>API Key Valid</span>
              </div>
            `
          : html`
              <div class="status-box error">
                <uui-icon name="icon-alert"></uui-icon>
                <span>API Key not found or not valid - please check Configuration</span>
              </div>
            `}

            ${this._stats
        ? html`
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
                  <div class="stat-value">${this.#formatTimeSaved(this._stats.timeSaved ?? 0)}</div>
                  <div class="stat-label">Time Saved</div>
                </div>
              </div>
            </uui-box>
          `
        : ""}

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
        ${this._spamComments.length === 0
          ? html`<p>No spam comments found.</p>`
          : html`
              <div class="comments-list">
                ${this._spamComments.slice(0, 10).map(
                  (comment) => html`
                    <div class="comment-item">
                      <div class="comment-header">
                        <strong>${comment.userName ?? "Anonymous"}</strong>
                        <span class="comment-date">${new Date(comment.commentDate).toLocaleString()}</span>
                      </div>
                      <div class="comment-text">${comment.commentText}</div>
                      <div class="comment-actions">
                        <uui-button
                          look="secondary"
                          label="Report Ham"
                          @click="${() => this.#onReportHam(comment.id)}"
                        >
                          Report as Not Spam
                        </uui-button>
                        <uui-button
                          look="primary"
                          color="danger"
                          label="Delete"
                          @click="${() => this.#onDeleteComment(comment.id)}"
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

  static styles = [
    css`
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
    `,
  ];
}

export default AkismetDashboardElement;

declare global {
  interface HTMLElementTagNameMap {
    "akismet-dashboard": AkismetDashboardElement;
  }
}
