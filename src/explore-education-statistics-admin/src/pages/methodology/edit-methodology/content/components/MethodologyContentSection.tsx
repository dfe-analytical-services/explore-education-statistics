import React from 'react';
import ContentBlocks, {
  EditableContentBlocksProps,
} from '@admin/components/editable/EditableContentBlocks';

interface MethodologyContentSectionProps extends EditableContentBlocksProps {
  isAnnex?: boolean;
}

const MethodologyContentSection = ({
  isAnnex = false,
}: MethodologyContentSectionProps) => {
  return (
    <>
      Hello World! {isAnnex ? 'isAnnex' : 'isNotAnnex'}
      {/* <ContentBlocks {...props}></ContentBlocks> */}
    </>
  );
};

export default MethodologyContentSection;
