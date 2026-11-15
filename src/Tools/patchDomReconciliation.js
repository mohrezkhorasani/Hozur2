/**
 * DOM Reconciliation Protection for React.
 * Prevents "NotFoundError: Failed to execute 'removeChild' on 'Node': The node to be removed is not a child of this node."
 * and "NotFoundError: Failed to execute 'insertBefore' on 'Node'".
 *
 * Root Cause: Google Translate and 3rd-party browser extensions mutate, wrap (<font>), or reparent
 * text nodes and DOM elements behind React's virtual DOM reconciler. When React attempts to commit deletions
 * or mutations, parentNode.removeChild(childNode) throws because parentNode is no longer childNode's parent.
 */
export function setupDomPatch() {
  if (typeof window === 'undefined' || typeof Node === 'undefined' || !Node.prototype) {
    return;
  }

  // Prevent multiple initializations
  if (window.__DOM_RECONCILIATION_PATCHED__) {
    return;
  }
  window.__DOM_RECONCILIATION_PATCHED__ = true;

  const originalRemoveChild = Node.prototype.removeChild;
  Node.prototype.removeChild = function (child) {
    if (child && child.parentNode !== this) {
      if (child.parentNode) {
        // Child was reparented or wrapped (e.g. by Google Translate <font> tag)
        return child.parentNode.removeChild(child);
      }
      // Child is already detached from DOM
      return child;
    }
    return originalRemoveChild.apply(this, arguments);
  };

  const originalInsertBefore = Node.prototype.insertBefore;
  Node.prototype.insertBefore = function (newNode, referenceNode) {
    if (referenceNode && referenceNode.parentNode !== this) {
      if (referenceNode.parentNode) {
        return referenceNode.parentNode.insertBefore(newNode, referenceNode);
      }
      return this.appendChild(newNode);
    }
    return originalInsertBefore.apply(this, arguments);
  };
}

// Automatically invoke on import
setupDomPatch();
