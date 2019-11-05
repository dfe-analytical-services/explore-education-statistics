### Tables & Charts & Datablocks

Tables and charts (inc. maps) and Summary items are added through the "DataBlock" component.

The idea being that it has a single request and then the items are in tabs and can pick from the results of that
single request, the data and metadata that they need.

This uses the Data API and the associated API interfaces.

The configuration for a DataBlock is stored in the Content database.

The data and metadata is used in combintation with the configuration to determine how to display the data.

```
Heading : string,
DataBlockRequest : Configuration of the Request
Charts : Array of charts
Summary : A summary block
Tables : Array of tables 
```

`DataBlockRequest` is configured as

```
    subjectId : number,
    geographicLevel : string,
    countries : string[],
    localAuthorities: string[],
    regions: string[],
    startYear: string,
    endYear: string,
    filters: string[],
    indicators: string[]
```

Most fields need filling to some expectation, the general expectation is that the `countries`, `localAuthorities` 
or `regions` are populated based on the geographicLevel, but it is a filter.

`Charts` is an array of Chart objects

```
{
    type : ( "line" | "horizontalbar" | "verticalbar" | "map" )
    indicators : string[]
    XAxis : Axis
    YAxis : Axis
}
```

The indicators are used to filter the results and only use those values. The `Map` type ignores the Axis
An Axis has a `title` and that is basically all at present. 

`Summary` is used by the "Key Stats" / Headline figures / Summary tab.

Each Summary item is

{
    dataKeys: string[]
    description: MarkdownBlock
}

`dataKeys` being the array of indicators (legacy naming)
description is a piece of editable content the content manager can add

`Tables` is an array of Table objects

```
{
    indicators: string[]
}
```

At present that's all a Table is. It requires the metadata to work out how to display the data.
