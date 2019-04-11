export interface PublicationSubjectSpecification {
  publicationId: string;
  subjects: {
    label: string;
    value: string;
  }[];
}

const publicationSubjectSpec: PublicationSubjectSpecification = {
  publicationId: '',
  subjects: [
    {
      label: 'Geographic levels',
      value: 'geoglevels',
    },
    {
      label: 'Local authority characteristics',
      value: 'lacharacteristics',
    },
    {
      label: 'National characteristics',
      value: 'natcharacteristics',
    },
  ],
};

export default publicationSubjectSpec;
