# Introduction

This repository contains a sample application to integrate assets from the [Aprimo DAM](https://www.aprimo.com/) to products in [Akeneo PIM](https://www.akeneo.com/).

When an asset is created or updated in Aprimo a [Rule](https://developers.aprimo.com/digital-asset-management/introduction/#module4) will be triggered which will create or update an asset in Akeneo with a public URL to the asset in Aprimo. Additional fields from the Aprimo asset can be synced to the Akeneo asset based on configuration. Additionally, the asset in Akeneo can be associated to a product in Akeneo based on configuration.

# Getting Started

In order to use this integration, you need to perform some required configuration in Akeneo and Aprimo.

## Akeneo Setup

### Asset Family

In Akeneo you need to set up your [Asset Families](https://api.akeneo.com/concepts/asset-manager.html#asset-family). Decide the asset families to which you want to sync assets from Aprimo and set up their attributes.

### Application Registration

Register the integration as an application in Akeneo:

https://api.akeneo.com/apps/create-custom-app.html

It is recommended to use a dedicated user for the integration that has permissions to create and update assets and products.

## Aprimo Setup

### Fields

In Aprimo you will need to create the fields you want to sync to Akeneo assets. Only `Text` field types are currently supported.

### Rule(s)

In addition, you will need to create a [Rule](https://developers.aprimo.com/digital-asset-management/introduction/#module4) to trigger the integration. The rule should be configured to trigger on the `Record/File` target whenever the record is saved or deleted. The rule should execute a [HTTP request reference](https://training3.dam.aprimo.com/assets/webhelp/adamhelp.htm#Admin%20Guide/References/httpRequest%20references.htm?TocPath=DAM%25C2%25A0Administration%257CUsing%2520References%257C_____15) that looks like the below.

You can also add additional conditions to execution of this rule based on your use case (for example, only execute the rule when the Akeneo Asset Family field is changed or only execute the rule for Assets in a certain classification).

```xml
<ref:text out="Basic [AUTH]" store="@apiKey" />

<ref:httpRequest uri="https://[INTEGRATION HOST]/aprimo/execute" type="application/json" hmacHeader="x-aprimo-hmac" retryCount="3" timeout="15">
   <Request>
      <Headers>
         <Header name="Authorization">@apiKey</Header>
         <Header name="x-akeneo-tenant">[AKENEO HOST]</Header>
      </Headers>
      <Body>
{
"recordId":<ref:text out="@adamRecord" encode="json" />,
"userId":<ref:user out="id" encode="json" />
"assetFamily":"[ASSET FAMILY]"
}
      </Body>
   </Request>
</ref:httpRequest>
```

Replace items in brackets (`[]`) with:

- [AUTH]: The integration endpoint is protected by HTTP Basic authentication to ensure it is only called by expected users. This value needs to be a base64-encoded value of the format `username:password`. The username and password are configured by you. See the [Security](#security) section for more information.
- [INTEGREATION HOST]: This is the host name of the integration endpoint. See the [Running the Integration](#running-the-integration) section for more information.
- [AKENEO HOST]: This is the host name of your Akeneo PIM environment (e.g. `aprimo.demo.cloud.akeneo.com`). This value must match other configuration values in the integration (see [Configuration](#configuration)).
- [ASSET FAMILY]: This is the asset family in Akeneo that you want to sync assets to for this rule. You can either hard-code this value or use a dynamic field lookup so this value is determined based on a field on the record. See [Dynamic Field Lookup in Aprimo Rule](#dynamic-field-lookup-in-aprimo-rule) for more information.

You can also add a `productId` field to the request body in the Aprimo rule. If this value is set, the asset will be associated to the product in Akeneo with the given ID. This value can also be a dynamic field lookup so the product ID is determined based on a field on the record. See [Dynamic Field Lookup in Aprimo Rule](#dynamic-field-lookup-in-aprimo-rule) for more information.

The `userId` field is needed to avoid an infinite loop since the integration may update the Aprimo record which could trigger the rule again.

#### Dynamic Field Lookup in Aprimo Rule

The `assetFamily` and `productId` values in the request body of the Aprimo rule can use a dynamic field lookup syntax to reference a field on the record. The format of this is:

`{{record.fields.[FIELD NAME]}}`

where [FIELD NAME] is replaced with the name of the field on the record. For example:

`{{record.fields.akeneoAssetFamily}}`

Or:

`{{record.fields.akeneoProductId}}`

### Integration Client

You will need to register an integration client in Aprimo so the integration can authenticate to the Aprimo API. See [Aprimo REST API Authorization](https://developers.aprimo.com/marketing-operations/rest-api/authorization/) for more information.

The registration should use the "Client Credential" OAuth flow. It is recommended to create a dedicated Aprimo user for the integration to use that has permissions to read and update records and create public links.

# Running the Integration

The integration is a standard ASP.NET 6 application that can be hosted in a variety of places. There's also a provided Dockerfile for building and running the application as a container.

The authentication tokens from Aprimo and Akeneo are stored via an implementation of `ITokenStorage`. The default storage is a file on disk encrypted via [`IDataProtector`](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/using-data-protection). This default may not make sense in production for your use case and we recommend implementing a different storage mechanism.

## Configuration

All configuration for the integration uses the standard [configuration mechanism of ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/). That means you can configure the integration using environment variables, command line arguments, or the appsettings.json configuration file.

The following configuration is required:

- `Akeneo:[AKENEO HOST]:ClientId`: The client ID of the Akeneo application. See [Akeneo App Authentication and Authorization](https://api.akeneo.com/apps/authentication-and-authorization.html).
- `Akeneo:[AKENEO HOST]:ClientSecret`: The client secret of the Akeneo application. See [Akeneo App Authentication and Authorization](https://api.akeneo.com/apps/authentication-and-authorization.html).
- `Akeneo:[AKENEO HOST]:FieldMappings`: The mapping of fields in Aprimo to attributes in Akeneo. See [Mapping Fields](#mapping-fields).
- `Aprimo:[APRIMO TENANT ID]:ClientId`: The client ID of the Aprimo integration registration. See [Aprimo REST API Authorization](https://developers.aprimo.com/marketing-operations/rest-api/authorization/).
- `Aprimo:[APRIMO TENANT ID]:ClientSecret`: The client secret of the Aprimo integration registration. See [Aprimo REST API Authorization](https://developers.aprimo.com/marketing-operations/rest-api/authorization/).
- `Aprimo:[APRIMO TENANT ID]:HMACSecret`: The secret used to generate the HMAC signature for the Aprimo rule. See [Security](#security).
- `Aprimo:[APRIMO TENANT ID]:BasicAuthPassword`: The password used for HTTP Basic authentication. See [Security](#security).

Where:

- `[AKENEO HOST]` is the host name of your Akeneo PIM environment (e.g. `aprimo.demo.cloud.akeneo.com`).
- `[APRIMO TENANT ID]` is your Aprimo environment sub-domain (e.g. mytenant.aprimo.com would be `mytenant`).

### Mapping Fields

Each Akeneo Asset Family requires separate configuration for mapping fields from Aprimo assets to Akeneo assets. This is configured in the `Akeneo:[AKENEO HOST]:FieldMappings` configuration value. The format of this value is:

`[ASSET FAMILY NAME]=[FIELD MAPPING]`

Where `[ASSET FAMILY NAME]` is the code of the Asset Family in Akeneo and `[FIELD MAPPING]` is a key/value pair mapping the Aprimo field to the Akeneo asset.

The Aprimo fields follow the format `record.fields.[FIELD NAME]` where `[FIELD NAME]` is the name of the field on the record. For example:

`record.fields.title`

In addition, the following values are available:

- `record.id`: The ID of the record in Aprimo.
- `record.publicUri`: A public URL to the asset in Aprimo.
- `syncDate`: The current date time in UTC format of when the sync ran.

Example mappings (using JSON format):

```json
{
  "packshots": {
    "record.publicUri": "asset.media_url",
    "record.fields.Record Description": "asset.description",
    "record.fields.Record Name": "asset.label"
  },
  "userguides": {
    "record.publicUri": "asset.media_url",
    "record.fields.Record Description": "asset.description",
    "record.fields.Record Name": "asset.label"
  }
}
```

### Updating Products

If you want the integration to associate the Akeneo Asset to an Akeneo Product after creation, the body of the rule request must include a `productId` field that is non-empty which matches the ID of a Product in Akeneo.

Additionally, the configuration for the Asset Family mapping must include a key called `AssetAttributeName` which is the code of the attribute on the Product that will store the Assets. For example:

```json
{
  "packshots": {
    "record.publicUri": "asset.media_url",
    "record.fields.Record Description": "asset.description",
    "record.fields.Record Name": "asset.label",
    "AssetAttributeName": "packshots"
  }
}
```

You can also sync Product attributes to the Aprimo Asset fields by including Product field mappings in the configuration of the mapping for the Asset Family. For example:

```json
{
  "packshots": {
    "record.publicUri": "asset.media_url",
    "record.fields.Record Description": "asset.description",
    "record.fields.Record Name": "asset.label",
    "AssetAttributeName": "packshots",
    "product.values.text": "record.description"
  }
}
```

All text attributes on the Product will be available in this format `product.values.[ATTRIBUTE CODE]` where `[ATTRIBUTE CODE]` is the code of the attribute on the Product.

The followingn fields are also available:

- `product.identifier`: The identifier of the Product in Akeneo.
- `syncDate`: The current date time in UTC format of when the sync ran.

### Full Configuration Example:

```json
{
  "Akeneo": {
    "test.demo.cloud.akeneo.com": {
      "ClientId": "1234",
      "ClientSecret": "my_secret",
      "FieldMappings": {
        "packshots": {
          "AssetAttributeName": "packshots",
          "record.publicUri": "asset.media_url",
          "record.fields.Record Description": "asset.description",
          "record.fields.Record Name": "asset.label"
        },
        "userguides": {
          "AssetAttributeName": "user_guides",
          "record.publicUri": "asset.media_url",
          "record.fields.Record Description": "asset.description",
          "record.fields.Record Name": "asset.label",
          "product.values.sku": "record.fields.Product SKU"
        }
      }
    }
  },
  "Aprimo": {
    "mytenant": {
      "ClientId": "9876",
      "ClientSecret": "my_client_secret",
      "HMACSecret": "hmacSecretFromAprimo",
      "BasicAuthPassword": "myBasicAuthPassword"
    }
  }
}
```

# Security

The integration endpoint is protected by two mechanisms to make sure only valid requests are processed:

- HTTP Basic authentication
- (Optionally) HMAC signature

The password for HTTP Basic authentication is configured at the configuration path `Aprimo:[APRIMO TENANT ID]:BasicAuthPassword` where `[APRIMO TENANT ID]` is your Aprimo environment sub-domain (e.g. mytenant.aprimo.com would be `mytenant`). The user name is also your Apriom environment sub-domain. As an example, if your Aprimo environment is `mytenant.aprimo.com` then you would configure the Basic authentication password in `Aprimo:mytenant:BasicAuthPassword`. If you set the value to `mysecretpassword` then the value you would use in the `Authorization` header of the Aprimo rule would be the base64-encoded value of `mytenant:mysecretpassword` which is: `bXl0ZW5hbnQ6bXlzZWNyZXRwYXNzd29yZA==`.

If configured, the HMAC signature of the request can also be verified. To enable verification of the HMAC signature, you must:

1. Configure the `.hmacSecret` setting in Aprimo DAM.
2. Configure the same value in the integration configuration at `Aprimo:[APRIMO TENANT ID]:HMACSecret`.
3. In the Aprimo rule on `httpRequest` set `hmacHeader="x-aprimo-hmac"` so that Aprimo will send the HMAC signature of the request in the HTTP header `x-aprimo-hmac`.

More information on HMAC signature support is available in Aprimo documentation:

https://training3.dam.aprimo.com/assets/webhelp/adamhelp.htm#Admin%20Guide/References/httpRequest%20references.htm

# Future Enhancement Opportunities

- Supporting other field types, especially localized fields
- A scheduled sync to sync data from Akeneo to Aprimo
