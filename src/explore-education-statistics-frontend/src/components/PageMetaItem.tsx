/* eslint-disable react/jsx-props-no-spreading */
import { Dictionary } from '@common/types';
import React from 'react';

export interface ScriptMetaItem {
  type: 'script';
  attributes?: Dictionary<string>;
  content?: string;
}
export interface MetaMetaItem {
  type: 'meta';
  attributes: Dictionary<string>;
  content?: undefined;
}

export type PageMetaItemType = ScriptMetaItem | MetaMetaItem;

export default function PageMetaItem({
  type,
  attributes,
  content,
}: PageMetaItemType) {
  switch (type) {
    case 'meta':
      return (
        <meta key={JSON.stringify({ type, attributes })} {...attributes} />
      );
    case 'script':
      return (
        <script key={JSON.stringify({ type, attributes })} {...attributes}>
          {content}
        </script>
      );
    default:
      return null;
  }
}
