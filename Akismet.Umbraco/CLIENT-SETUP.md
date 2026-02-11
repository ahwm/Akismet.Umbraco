# Akismet for Umbraco 17 - Frontend Setup

This document describes the frontend TypeScript/Lit implementation for the Akismet dashboard.

## Architecture

The frontend is built using:
- **Lit** - Web components framework
- **TypeScript** - Type safety
- **Umbraco Backoffice UI** - Official Umbraco UI library  
- **Vite** - Build tool

## Files Structure

```
Client/
├── src/
│   ├── api/              # Auto-generated API client
│   │   ├── client.gen.ts
│   │   ├── sdk.gen.ts    # Service methods for API calls
│   │   └── types.gen.ts
│   ├── dashboards/
│   │   ├── dashboard.element.ts  # Main Akismet dashboard component
│   │   └── manifest.ts           # Dashboard registration
│   ├── entrypoints/
│   │   ├── entrypoint.ts
│   │   └── manifest.ts
│   └── bundle.manifests.ts
├── scripts/
│   └── generate-openapi.js  # OpenAPI client generator
└── vite.config.ts
```

## Dashboard Features

The Akismet dashboard (`dashboard.element.ts`) provides:

1. **API Key Verification** - Shows if the configured API key is valid
2. **Statistics Display** - Shows Akismet stats (spam blocked, accuracy, time saved)
3. **Database Statistics** - Local spam/ham counts
4. **Spam Comments List** - Recent spam comments with actions:
   - Report as Ham (false positive)
   - Delete comment

## API Client

The TypeScript API client is manually configured in `src/api/sdk.gen.ts` with the following endpoints:

- `verifyKey()` - Verify API key
- `getStats()` - Get Akismet statistics  
- `getComments()` - Get all comments
- `getSpamComments()` - Get spam comments only
- `getSpamCount()` - Get spam count
- `getHamCount()` - Get ham count
- `deleteComment(id)` - Delete a comment
- `reportHam(id)` - Report false positive
- `reportSpam(id)` - Report missed spam
- `checkComment(comment)` - Check if comment is spam

## Building

To build the frontend assets:

```bash
cd Client
npm install
npm run build
```

This will:
1. Run TypeScript compiler (`tsc`)
2. Bundle with Vite
3. Output to `wwwroot/App_Plugins/AkismetUmbraco/`

## Development

For development with hot reload:

```bash
cd Client
npm run dev
```

Then access your Umbraco backoffice at the configured URL.

## Regenerating API Client

When you add new API endpoints, you can regenerate the TypeScript client:

1. Start your Umbraco instance
2. Run: `npm run generate-openapi`
3. This fetches the OpenAPI spec from your running Umbraco instance
4. Generates TypeScript types and service methods

Note: The OpenAPI URL is configured in `package.json` under the `generate-openapi` script.

## Dashboard Location

The dashboard is registered to appear in the **Settings** section of the Umbraco backoffice at `/settings/dashboard/akismet`.

## Styling

The dashboard uses Umbraco's UI library components (`uui-*`) and follows Umbraco's design system with custom CSS for:
- Status boxes (success/error)
- Statistics grid
- Comment cards
- Action buttons

## State Management

The dashboard uses Lit's `@state()` decorator for reactive state:
- `_keyValid` - API key validation status
- `_stats` - Akismet statistics
- `_spamComments` - List of spam comments
- `_loading` - Loading state
- `_spamCount` / `_hamCount` - Database counts

All data is loaded on component initialization via the `loadData()` method.

## Notifications

The dashboard integrates with Umbraco's notification system (`UMB_NOTIFICATION_CONTEXT`) to show:
- Success messages after actions
- Error messages when operations fail

## Future Enhancements

Possible future additions:
- Pagination for spam comments list
- Bulk actions (delete/report multiple)
- Filtering and search
- Real-time updates via SignalR
- Comment detail modal
- Export functionality
