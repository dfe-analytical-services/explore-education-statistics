// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
import React, { ChangeEvent, Fragment } from 'react';

// import 'react-draft-wysiwyg/dist/react-draft-wysiwyg.css';

interface Props {
  content: string;
}

interface State {
  content: string;
  editing: boolean;
  unsaved: boolean;
}

export class PrototypeEditableContent extends React.Component<Props, State> {
  private ref: HTMLElement | null = null;

  private temporaryContent: string = '';

  public state: State = {
    content: '',
    editing: false,
    unsaved: false,
  };

  public componentDidMount() {
    this.setState({
      content: this.props.content,
      editing: false,
      unsaved: false,
    });

    this.temporaryContent = this.props.content;

    if (!this.state.editing && this.ref) {
      this.ref.innerHTML = this.state.content;
    }
  }

  public componentDidUpdate() {
    if (!this.state.editing && this.ref) {
      this.ref.innerHTML = this.state.content;
    }
  }

  public setEditing = () => {
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
    return (
      <Fragment>
        {this.state.editing ? (
          <div className="editable-content editable-content-editing">
            <div className="editable-button">
              <button onClick={this.save}>Save</button>
            </div>
            <CKEditor
              editor={ClassicEditor}
              data={this.state.content}
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
            {this.state.unsaved ? (
              <div className="editable-button unsaved">Click to edit</div>
            ) : (
              <div className="editable-button">Click to edit</div>
            )}
            <div ref={ref => (this.ref = ref)} />
          </div>
        )}
      </Fragment>
    );
  }
}
