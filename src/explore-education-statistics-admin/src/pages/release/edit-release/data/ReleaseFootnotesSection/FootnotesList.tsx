import React, { useEffect, useState } from 'react';
import { PublicationSubjectMeta } from '@common/modules/full-table/services/tableBuilderService';
import { FootnoteMeta, Footnote } from '.';

interface Props {
  footnoteMeta: FootnoteMeta;
  footnotes: Footnote[];
}

const FootnotesList = ({ footnotes, footnoteMeta }: Props) => {
  return footnotes.length === 0 ? null : <>Footnote Table here.</>;
};

export default FootnotesList;
