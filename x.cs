                    DynamicObject moFCATEGORYIDVAlue = moMATERIALBASE["CATEGORYID"] as DynamicObject;
                    var moCategoryId = moFCATEGORYIDVAlue["id"].ToString();
                                        Debug.Print(moCategoryId + ":");
                                                                DynamicObject categoryObj = materialPPBOMENTRY["CATEGORYID"] as DynamicObject;
                                                                var entryPPBOMcategoryId = categoryObj["Id"].ToString();
                        Debug.Print(entryPPBOMcategoryId);
                                                if (moCategoryId == entryPPBOMcategoryId)
                        {
                            find = true;
                            break;

                        }