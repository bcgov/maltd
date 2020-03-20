export function checkArrayEquality(arr1, arr2) {
  // Check if the arrays are the same length
  if (arr1.length !== arr2.length) return false;

  // Check if all items exist and are in the same order
  for (var i = 0; i < arr1.length; i++) {
    if (arr1[i].type !== arr2[i].type || arr1[i].status !== arr2[i].status)
      return false;
  }

  // Otherwise, return true
  return true;
}
