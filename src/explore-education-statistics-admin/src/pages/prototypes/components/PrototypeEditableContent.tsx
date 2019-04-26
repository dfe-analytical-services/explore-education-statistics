// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
import React, { ChangeEvent, createRef, Fragment } from 'react';

// import 'react-draft-wysiwyg/dist/react-draft-wysiwyg.css';

interface Props {
  content: string;
}

interface State {
  content: string;
  editing: boolean;
  unsaved: boolean;
}

export default class PrototypeEditableContent extends React.Component<
  Props,
  State
> {
  private ref = createRef<HTMLDivElement>();

  private temporaryContent: string = '';

  public state: State = {
    content: '',
    editing: false,
    unsaved: false,
  };

  public componentDidMount() {
    const { content } = this.props;

    this.setState({
      content,
      editing: false,
      unsaved: false,
    });

    this.temporaryContent = content;

    const { editing, content: content1 } = this.state;
    if (!editing && this.ref.current) {
      this.ref.current.innerHTML = content1;
    }
  }

  public componentDidUpdate() {
    const { editing, content } = this.state;

    if (!editing && this.ref.current) {
      this.ref.current.innerHTML = content;
    }
  }

  public setEditing = () => {
    // eslint-disable-next-line react/destructuring-assignment
    if (!this.state.editing) {
      this.setState({ editing: true });
    }
  };

  public save = () => {
    this.setState({
      editing: false,
      unsaved: true,
      content: this.temporaryContent,
    });
  };

  public render() {
    const { editing, unsaved, content } = this.state;

    return (
      <Fragment>
        {editing ? (
          <div className="editable-content editable-content-editing">
            <div className="editable-button">
              <button onClick={this.save} type="submit">
                Save
              </button>
            </div>
            <CKEditor
              editor={ClassicEditor}
              data={content}
              onChange={(event: ChangeEvent, editor: { getData(): string }) => {
                this.temporaryContent = editor.getData();
              }}
              onInit={(editor: { editing: { view: { focus(): void } } }) => {
                editor.editing.view.focus();
              }}
            />
          </div>
        ) : (
          // eslint-disable-next-line jsx-a11y/no-static-element-interactions,jsx-a11y/click-events-have-key-events
          <div className="editable-content" onClick={this.setEditing}>
            {unsaved ? (
              <div className="editable-button unsaved">Click to edit</div>
            ) : (
              <div className="editable-button">Click to edit</div>
            )}
            <div ref={this.ref} />
          </div>
        )}
      </Fragment>
    );
  }
}
